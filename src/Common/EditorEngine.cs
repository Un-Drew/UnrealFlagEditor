using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using UELib;
using UELib.Core;

namespace UnrealFlagEditor
{
    public class PropertyChangeMadeEventArgs : EventArgs
    {
        // The property whose value was modified.
        public EdProp_Base Property;

        public PropertyChangeMadeEventArgs(EdProp_Base property) : base()
        {
            Property = property;
        }
    }

    public class EditorEngine : IDisposable
    {
        static public System.Collections.Generic.IComparer<EdNode_Base> NodeComparer = new EdNodeSorter_List();

        [Flags]
        public enum EditorLoadFlags
        {
            NeedHierarchy = 0x01,
            SortNodes = 0x02, // Requires NeedHierarchy
            UpdateStatsAfterEachChange = 0x04,

            Headless = NeedHierarchy,
            Form = NeedHierarchy | SortNodes | UpdateStatsAfterEachChange
        }

        public EditorLoadFlags LoadFlags;

        // First type: UELib class, Second type: Editor class.
        static public Dictionary<Type, Type> EditorTypes = new Dictionary<Type, Type>();
        static public bool HasInitializedEditorTypes;

        public UnrealPackage LoadedPackage;

        /**
         * Root for the package tree. Can be iterated recursively via ChildrenEdNodes, which is sorted
         * (as long as the SortNodes flag was added).
         * Use this to iterate recursively if the parent-child order matters.
         * Keep in mind, this is about 3 times slower than using the AllNodes list. Use this sparingly.
         */
        public EdNode_Base PackageRoot;
        /**
         * List of all nodes, unordered. Use this if the iteration order doesn't matter.
         */
        public List<EdNode_Base> AllNodes = new List<EdNode_Base>();

        public uint CachedChangeCount;
        public bool StatsNeedUpdating;

        public event EventHandler<PropertyChangeMadeEventArgs> PropertyChangeMade;
        public event EventHandler StatsUpdated;

        public EditorEngine(EditorLoadFlags loadFlags)
        {
            LoadFlags = loadFlags;

            RegisterEditorClassTypes();
            UpdateStats();
        }

        public void RegisterEditorClassTypes()
        {
            if (HasInitializedEditorTypes) return;

            var exportedTypes = System.Reflection.Assembly.GetExecutingAssembly().GetExportedTypes();
            foreach (var exportedType in exportedTypes)
            {
                object[] attributes = exportedType.GetCustomAttributes(typeof(EditorRegisterClassAttribute), false);
                foreach (EditorRegisterClassAttribute attribute in attributes)
                {
                    attribute.AddAffectedClasses(exportedType, EditorTypes);
                }
            }

            HasInitializedEditorTypes = true;
        }

        public void DisposeLoadedPackage()
        {
            if (LoadedPackage == null) return;

            foreach (var node in AllNodes)
            {
                node.Dispose();
            }
            PackageRoot = null;
            AllNodes.Clear();

            LoadedPackage.Dispose();
            LoadedPackage = null;
        }

        public void LoadPackage(string path)
        {
            if (LoadedPackage != null)
            {
                throw new ApplicationException("Cannot load package, because a package is already loaded.");
            }


            // HACK: Temporarily remove the console TextWriter so the package file summary doesn't get show up.
            TextWriter originalConsoleOut = Console.Out;
            Console.SetOut(TextWriter.Null);

            try
            {
                LoadedPackage = UnrealLoader.LoadPackage(path, System.IO.FileAccess.ReadWrite);

                Console.SetOut(originalConsoleOut);
                originalConsoleOut = null;

                if (LoadedPackage.Summary.CompressedChunks != null && LoadedPackage.Summary.CompressedChunks.Capacity > 0)
                {
                    throw new FileLoadException("Compressed packages are not supported! Use the uncompressed version of this package, if available.");
                }

                if (LoadedPackage.Summary.UE4Version != 0)
                {
                    throw new FileLoadException("Packages from UE4 or later are not supported!");
                }

#if ALWAYS_ONLY_DESERIALIZE_ON_DEMAND
                LoadedPackage.InitializePackage(UnrealPackage.InitFlags.All & (~UnrealPackage.InitFlags.Deserialize));
#else
                LoadedPackage.InitializePackage(UnrealPackage.InitFlags.All);
#endif

                InitializeNodes();
                UpdateStats();
            }
            catch
            {
                // Oops, something went wrong! Get that thing out of here.
                DisposeLoadedPackage();
                UpdateStats();

                if (originalConsoleOut != null)
                    Console.SetOut(originalConsoleOut);

                throw;
            }
        }

        public void InitializeNodes()
        {
            /*
             * Create the root node for the package.
             */
            PackageRoot = new EdNode_RootPackage(this, LoadedPackage);
            PackageRoot.PreInitNode();

            AllNodes.Add(PackageRoot);

            /*
             * Create the nodes for the objects.
             */
            Dictionary<UObject, EdNode_UObject> objectNodes = new Dictionary<UObject, EdNode_UObject>();
            EdNode_UObject currentNode;
            {
                Type editorType;

                foreach (UObject obj in LoadedPackage.Objects)
                {
                    if (obj.ExportTable == null) continue;

                    EditorTypes.TryGetValue(obj.GetType(), out editorType);
                    if (editorType == null)
                    {
#if SKIP_UNNECESSARY_NODES
                        continue;
#else
                        editorType = typeof(EdNode_Dummy);
#endif
                    }

                    currentNode = (EdNode_UObject)Activator.CreateInstance(editorType, this, obj);

                    AllNodes.Add(currentNode);
                    objectNodes.Add(obj, currentNode);
                }
            }

            if ((LoadFlags & EditorLoadFlags.NeedHierarchy) != 0)
            {
                /*
                 * Next, parent the UObject ed nodes into a hierarchy. For items in the middle of the UObject hierarchy that
                 * don't have specialized ed nodes yet, just create some dummy nodes.
                 */
                {
                    UObject outerObj;
                    EdNode_UObject foundNode;
                    // It's dangerous to iterate through something while adding items to it, so make a copy of the dictionary's values.
                    List<EdNode_UObject> objectNodes_Iter = new List<EdNode_UObject>(objectNodes.Values);
                    foreach (EdNode_UObject node in objectNodes_Iter)
                    {
                        currentNode = node;
                        outerObj = currentNode.OwnerObject.Outer;
                        do
                        {
                            if (outerObj == null)
                            {
                                // No outer - put this in the root package.
                                PackageRoot.AddEdNodeChild(currentNode);
                                break;
                            }

                            if (objectNodes.TryGetValue(outerObj, out foundNode))
                            {
                                // Found the node for this outer - parent to it.
                                foundNode.AddEdNodeChild(currentNode);
                                break;
                            }

                            // Our outer is of an unregistered class, and it doesn't have a node yet. Add it as a dummy,
                            // because one of its children is needed.
                            foundNode = new EdNode_Dummy(this, outerObj);

                            AllNodes.Add(foundNode);
                            objectNodes.Add(outerObj, foundNode);

                            foundNode.AddEdNodeChild(currentNode);

                            // Repeat this process with each dummy, until we've climbed up to something known (like the root).
                            currentNode = foundNode;
                            outerObj = outerObj.Outer;
                        }
                        while (true);
                    }
                }

                if ((LoadFlags & EditorLoadFlags.SortNodes) != 0)
                    PackageRoot.SortEdNodeChildren(NodeComparer, recursive: true);
            }
        }

        public void OnPropertyChangeMade(EdProp_Base property)
        {
            if ((LoadFlags & EditorLoadFlags.UpdateStatsAfterEachChange) != 0)
                UpdateStats();
            else
                StatsNeedUpdating = true;
            
            PropertyChangeMade?.Invoke(this, new PropertyChangeMadeEventArgs(property));
        }

        public void ConditionalUpdateStats()
        {
            if (StatsNeedUpdating)
                UpdateStats();
        }

        public void UpdateStats()
        {
            StatsNeedUpdating = false;
            CachedChangeCount = GetChangesCount();
            StatsUpdated?.Invoke(this, EventArgs.Empty);
        }

        public uint GetChangesCount()
        {
            if (LoadedPackage == null) return 0;
            uint count = 0;
            foreach (EdNode_Base node in AllNodes)
            {
                count += node.CountChanges();
            }
            return count;
        }

        public void SaveOverwrite()
        {
            bool anyChange = false;
            try
            {
                foreach (EdNode_Base node in AllNodes)
                {
                    if (node.HasAnyChanges())
                    {
                        anyChange = true;
                        node.SaveChanges();
                    }
                }
                LoadedPackage.Stream.Flush(true);
            }
            catch
            {
                if (anyChange)
                {
                    // I don't think there's any way to tell a stream to cancel any pending changes, so,
                    // to avoid any unwanted, delayed behaviour, try to save whatever was already written,
                    // since that's gonna happen sooner or later anyway. :/
                    try
                    {
                        LoadedPackage.Stream.Flush(true);
                    }
                    catch { }
                }

                throw;
            }

            foreach (EdNode_Base node in AllNodes)
            {
                node.ApplyToDefault();
            }
            UpdateStats();
        }

        // Should happen on Save As.
        public bool MigrateLoadedPackageToNewFile(string path)
        {
            return PackageMigrateToNewFile(LoadedPackage, path);
        }

        // Copies the loaded package to a new file and migrates to it (keeps it in use, and frees up the original file).
        // Returns false if the new path is identical to the current one, and nothing happened.
        // Does not explicitly create the directory, assumes it already exists. May fail with an exception!
        static public bool PackageMigrateToNewFile(UnrealPackage package, string path)
        {
            if (string.Equals(Path.GetFullPath(package.Stream.Name), Path.GetFullPath(path), StringComparison.OrdinalIgnoreCase))
                return false;  // they're the same file.

            UPackageStream oldstream = package.Stream;
            UPackageStream stream = null;

            try
            {
                stream = new UPackageStream(path, FileMode.Create, FileAccess.ReadWrite);

                stream.Position = 0;
                oldstream.Position = 0;
                EndianAgnosticStreamCopy(oldstream, stream);
                stream.Position = 0;

                package.Stream = stream;
                stream.PostInit(package);  // Let it calculate its BigEndianCode

                stream.Position = oldstream.Position;
                stream.LastPosition = oldstream.LastPosition;  // TODO: is this even necessary? It's not used at all.

                oldstream.Close();

                return true;
            }
            catch
            {
                // In case something went wrong super late in the process, restore the old stream.
                if (package.Stream != oldstream)
                    package.Stream = oldstream;
                if (stream != null)
                    stream.Close();

                throw;
            }
        }

        // As of 1.5.0, UPackageStream.Read flips bytes in case of big-endian packages, which is a big no-no, so use the endian agnostic alternative.
        static public void EndianAgnosticStreamCopy(UPackageStream source, UPackageStream destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }

            if (!source.CanRead && !source.CanWrite)
            {
                throw new ObjectDisposedException(null, GetCoreResourceManager().GetString("ObjectDisposed_StreamClosed"));
            }

            if (!destination.CanRead && !destination.CanWrite)
            {
                throw new ObjectDisposedException("destination", GetCoreResourceManager().GetString("ObjectDisposed_StreamClosed"));
            }

            if (!source.CanRead)
            {
                throw new NotSupportedException(GetCoreResourceManager().GetString("NotSupported_UnreadableStream"));
            }

            if (!destination.CanWrite)
            {
                throw new NotSupportedException(GetCoreResourceManager().GetString("NotSupported_UnwritableStream"));
            }

            byte[] array = new byte[81920];
            int count;
            while ((count = source.EndianAgnosticRead(array, 0, array.Length)) != 0)
            {
                destination.Write(array, 0, count);
            }
        }

        static System.Resources.ResourceManager CoreResourceManager = null;

        static public System.Resources.ResourceManager GetCoreResourceManager()
        {
            if (CoreResourceManager == null)
            {
                var assembly = System.Reflection.Assembly.GetAssembly(typeof(object));
                CoreResourceManager = new System.Resources.ResourceManager(assembly.GetName().Name, assembly);
            }
            return CoreResourceManager;
        }

        static public string GetUserFriendlyFileError(string filePath, string friendlyTypeName, Exception e)
        {
            // Special case, to add a bit of info.
            if (e is UnauthorizedAccessException)
            {
                return $"Failed to open {friendlyTypeName}: {e.Message} Make sure that the file is not read-only, and that this user has access to it.";
            }
            // Exceptions that DO already specify the file path, or specifying it would be redundant:
            if (
                   e is ArgumentException
                || e is FileNotFoundException
                || e is SecurityException
                || e is DirectoryNotFoundException
                )
            {
                return $"Failed to open {friendlyTypeName}: {e.Message}";
            }
            // Exceptions that don't specify the file path:
            else if (
                   e is PathTooLongException
                // Thrown by UELib when trying to load something that isn't a package.
                || e is FileLoadException
                )
                {
                    return $"Failed to open {friendlyTypeName} {filePath}: {e.Message}";
                }
            return null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // ...
            }
            // If UnrealPackage doesn't call Dispose on its destruction, does that make it unmanaged? Idk.
            // At least, this seems to be the case as of 1.5.0.
            DisposeLoadedPackage();
        }

        ~EditorEngine()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    // Attribute that should be added at the top of every specialized EdNode definition - except for RootPackage and Dummy which are handled separately.
    [UELib.Annotations.MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class EditorRegisterClassAttribute : Attribute
    {
        public Type ForClass;
        public int Priority;
        public bool AffectSubclasses;

        public const int EDPR_DEFAULT = 10;

        public const int EDPR_UPROPERTY = 30;

        public const int EDPR_UOBJECT = 50;

        public EditorRegisterClassAttribute(Type inForClass, int inPriority = EDPR_DEFAULT, bool inAffectSubclasses = false) : base()
        {
            ForClass = inForClass;
            Priority = inPriority;
            AffectSubclasses = inAffectSubclasses;
        }

        public void AddAffectedClasses(Type ownerType, Dictionary<Type, Type> EditorTypes)
        {
            EditorTypes.Add(ForClass, ownerType);
            if (AffectSubclasses)
            {
                Func<Type, bool> func;
                if (ForClass.IsInterface)
                    func = type => type.IsClass && ForClass.IsAssignableFrom(type);
                else
                    func = type => type.IsSubclassOf(ForClass);
                // Search for sub-classes in all assemblies.
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (Type subclass in assembly.GetTypes().Where(func))
                    {
                        EditorTypes.Add(subclass, ownerType);
                    }
                }
            }
        }
    }

    // Sorts the nodes by type (and alphabetically, when appropriate).
    public class EdNodeSorter_List : System.Collections.Generic.IComparer<EdNode_Base>
    {
        public int Compare(EdNode_Base x, EdNode_Base y)
        {
            int xPriority = x.SortPriority;
            int yPriority = y.SortPriority;

            if (xPriority != yPriority)
                return xPriority - yPriority;

            if (x.SortAlphabetically && y.SortAlphabetically)
                return string.Compare(x.Text, y.Text);

            if (x.GetType() == y.GetType())
                return 0;

            return string.Compare(x.GetType().ToString(), y.GetType().ToString());
        }
    }
}
