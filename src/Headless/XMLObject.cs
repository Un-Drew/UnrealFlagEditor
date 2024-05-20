using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace UnrealFlagEditor
{
    [XmlType(TypeName = "Object")]
    public class XMLObject
    {
        // The reference path for this object.
        [XmlAttribute]
        public string ObjectPath;

        // If false, an error will be thrown in the console saying that the object couldn't be found.
        [XmlAttribute]
        public bool MightNotExist;

        // TODO: This is kinda hacky and ugly, but otherwise the deserializer demands the internal class name, which isn't very user-friendly. :/
        [XmlArrayItem(ElementName = "Flag", Type = typeof(XMLProperty_Flag))]
        public List<XMLProperty_Base> ModifyProperties = new List<XMLProperty_Base>();

        [XmlIgnore]
        public HeadlessInstructions Owner;

        [XmlIgnore]
        public int LineNumber;

        static char[] ObjectPathSeparators = new char[] {
            '.',
            ':'  // UnrealEd uses a colon instead of period before subobjects that don't belong to a package or a sub-package (i.e. group), so handle that too.
        };

        public XMLObject()
        {
            // HACK: Get the current line number from the globally assigned reader, on construction.
            //       Maybe there's a better/simpler way to do this?
            if (EditorHeadless.CurrentReader != null && EditorHeadless.CurrentReader is IXmlLineInfo)
            {
                LineNumber = ((IXmlLineInfo)EditorHeadless.CurrentReader).LineNumber;
            }
        }

        public EdNode_Base FindObject(EditorEngine edEngine)
        {
            try
            {
                DeconstructObjectPath(ObjectPath, out string[] pathElements, out string type);
                EdNode_Base node = StaticFindObject(edEngine, pathElements, type, throwOnAmbiguities: true, throwOnMismatchedType: true);
                if (node != null)
                {
                    // Make sure we have the node initialized.
                    node.InitNode();
                    return node;
                }
                else if (MightNotExist)
                    return null;
                else
                    throw new HeadlessException($"Could not find object {ObjectPath}!" +
                        $" Consider adding MightNotExist=\"true\" for this tag."
                    );
            }
            // Thrown when an object is found but the cast is incorrect.
            catch (InvalidCastException e)
            {
                throw new HeadlessException(e.Message, e);
            }
            // Thrown when an object path has multiple possible results and it's not quite clear which one should be used.
            catch (System.Reflection.AmbiguousMatchException e)
            {
                throw new HeadlessException(e.Message, e);
            }
        }

        public void ApplyProperties(EdNode_Base node)
        {
            if (!node.HasInitializedProperties)
            {
                node.PreInitProperties();
                node.HasInitializedProperties = true;
            }

            EdProp_Base prop;
            bool result;
            foreach (XMLProperty_Base xmlprop in ModifyProperties)
            {
                xmlprop.Owner = this;
                try
                {
                    prop = xmlprop.FindProperty(node);
                    if (prop != null)
                    {
                        result = xmlprop.ApplyProperty(prop);
                        if (Owner.PrintEachChange)
                        {
                            if (result)
                            {
                                EditorHeadless.RegisterConsoleMessage(
                                    $"CHANGE : {ObjectPath} - {xmlprop.Name} = {xmlprop.GetStringifiedValue()}.",
                                    HeadlessMessage.MessageType.Success
                                );
                            }
                            else
                            {
                                EditorHeadless.RegisterConsoleMessage(
                                    $"NO CHANGE : {ObjectPath} - {xmlprop.Name} was already {xmlprop.GetStringifiedValue()}.",
                                    HeadlessMessage.MessageType.Notification
                                );
                            }
                        }
                    }

                    if (Owner != null)
                    {
                        if (prop == null)
                            Owner.PropertiesNotFound++;
                        else
                            Owner.AttemptedChanges++;
                    }
                }
                catch (HeadlessException e)
                {
                    EditorHeadless.RegisterConsoleError(xmlprop.LineNumber, e);
                    if (Owner != null)
                        Owner.PrintEachChange = false;
                }
            }
        }

        public void ExtractProperty(EdNode_Base node)
        {
            EdProp_Base prop;
            foreach (XMLProperty_Base xmlprop in ModifyProperties)
            {
                xmlprop.Owner = this;
                try
                {
                    prop = xmlprop.FindProperty(node);
                    if (prop != null)
                        xmlprop.ExtractProperty(prop);
                    else if (Owner != null)
                        Owner.ObjectsNotFound++;
                }
                catch (HeadlessException e)
                {
                    EditorHeadless.RegisterConsoleError(LineNumber, e);
                }
            }
        }

        /**
         * Parses an object reference string, handling some of its syntax quirks. If no cast is present in the path, type will be null.
         * All output strings are invariant-uppercase, so they can be easily compared.
         * 
         * Throws HeadlessException if something went wrong.
         */
        static public void DeconstructObjectPath(string path, out string[] pathElements, out string type)
        {
            if (path == null)
            {
                throw new HeadlessException($"Object path is null!");
            }

            path = path.Trim();

            if (path.Length == 0)
            {
                throw new HeadlessException($"Object path is empty!");
            }

            if (path.Any(x => Char.IsWhiteSpace(x)))
            {
                throw new HeadlessException($"Invalid whitespace found in object path \"{path}\"! Object paths may not contain whitespace (such as spaces).");
            }

            int apostrophePos = path.IndexOf('\'');
            if (apostrophePos != -1)
            {
                if (path.Length - apostrophePos <= 1 || path.Last() != '\'')
                {
                    throw new HeadlessException($"Invalid or unclosed cast apostrophe found in object path \"{path}\"!");
                }

                if (path.Length - apostrophePos == 2)
                {
                    // Handle the case where there's a blank cast as well.
                    throw new HeadlessException($"Object path is empty!");
                }

                // Remove the cast + its apostrophes, so we can work with the path itself.
                string newPath = path.Substring(apostrophePos + 1, path.Length - apostrophePos - 2);

                if (newPath.Contains('\''))
                {
                    throw new HeadlessException($"Invalid number of cast apostrophes {2 + newPath.Count(x => x == '\'')} in object path \"{path}\"!");
                }

                if (apostrophePos == 0)
                    type = null;
                else
                    type = path.Substring(0, apostrophePos).ToUpperInvariant();

                path = newPath;
            }
            else
            {
                type = null;
            }

            pathElements = path.ToUpperInvariant().Split(ObjectPathSeparators, StringSplitOptions.RemoveEmptyEntries);
        }

        /**
         * Finds an object using a desconstructed object path. All strings are expected to be invariant-uppercase.
         * Handles the case where the package is explicitly mentioned at the beginning, as well as the case when it isn't.
         * 
         * bool throwOnAmbiguities - If the type is null and an ambiguity arises (i.e. if both searches return something),
         * throw a System.Reflection.AmbiguousMatchException. If false, it prioritizes the case where the package name is
         * explicitly mentioned.
         * 
         * bool throwOnMismatchedType - If an object is found but it's not of the correct type, throw an InvalidCastException.
         * If false, null will be returned instead.
         */
        static public EdNode_Base StaticFindObject(EditorEngine edEngine, string[] pathElements, string type = null, bool throwOnAmbiguities = false, bool throwOnMismatchedType = false)
        {
            EdNode_Base foundExplicit = null;

            if (string.Compare(edEngine.PackageRoot.Name, pathElements[0]) == 0)
            {
                // Assume the package name is explicitly mentioned first.
                foundExplicit = StaticFindObject_Sub(edEngine, pathElements, startFrom: 1);

                if (foundExplicit != null && type != null && foundExplicit.IsOfUnrealType(type))
                {
                    /*
                        In case a type was mentioned, and the more verbose variant matches it, then exit out early to prioritize it.
                        This is so, in the cosmically unlikely event that a sub-package (group) is named the same as its root owner,
                        the user is still able to reference either one.
                    */
                    return foundExplicit;
                }
            }

            EdNode_Base foundImplicit = StaticFindObject_Sub(edEngine, pathElements, startFrom: 0);

            if (foundImplicit != null && foundExplicit != null)
            {
                if (type == null)
                {
                    if (throwOnAmbiguities)
                    {
                        throw new System.Reflection.AmbiguousMatchException(
                        $"Object path is ambiguous, and can mean either {foundImplicit.GetReferencePath()} or {foundExplicit.GetReferencePath()}!" +
                        (
                            (string.Compare(foundImplicit.GetUnrealTypeString(), foundExplicit.GetUnrealTypeString()) != 0)
                            // Can be triggered in Engine.u when writing "Engine", as it could mean "Package'Engine'" or "Class'Engine'".
                            ? " Please specify a type in your object path."
                            // Incredibly unlikely, but possible in case a sub-package is named the same as its root owner.
                            : $" Please specity the object's type, and write the full object path, including the root {edEngine.PackageRoot.Text}."
                        ));
                    }
                    else
                        return foundExplicit;
                }

                // At this point, we know Explicit didn't have the right type. Let's see if this does.
                if (!foundImplicit.IsOfUnrealType(type))
                {
                    if (throwOnMismatchedType)
                        throw new InvalidCastException(ObjectTypeMismatchError(foundExplicit, foundImplicit, type));
                    else
                        return null;
                }

                return foundImplicit;
            }

            if (foundImplicit == null)
            {
                if (foundExplicit == null)
                    // Nothing was found, either implicitly or explicitly. Let the caller handle that.
                    return null;

                if (type != null)
                {
                    // If we got here, then we know Explicit didn't have the right type.
                    if (throwOnMismatchedType)
                        throw new InvalidCastException(ObjectTypeMismatchError(foundExplicit, type));
                    else
                        return null;
                }
                
                return foundExplicit;
            }

            if (!foundImplicit.IsOfUnrealType(type))
            {
                if (throwOnMismatchedType)
                    throw new InvalidCastException(ObjectTypeMismatchError(foundImplicit, type));
                else
                    return null;
            }

            return foundImplicit;
        }

        static public string ObjectTypeMismatchError(EdNode_Base node, string type)
        {
            return $"Found object {node.GetReferencePath()}, but it's not of the required type {type}!";
        }

        static public string ObjectTypeMismatchError(EdNode_Base node1, EdNode_Base node2, string type)
        {
            return $"Found objects {node1.GetReferencePath()} and {node2.GetReferencePath()}, but neither of them are of the required type {type}!";
        }

        static public EdNode_Base StaticFindObject_Sub(EditorEngine edEngine, string[] pathElements, int startFrom = 0)
        {
            EdNode_Base current = edEngine.PackageRoot;

            for (var i = startFrom; i < pathElements.Length; ++i)
            {
                current = current.ChildrenEdNodes.Find(n =>
                    // TODO: Is there a specific StringComparison or CompareOptions that should be used here?
                    string.Compare(n.Name, pathElements[i]) == 0);
                if (current == null)
                    return null;
            }

            return current;
        }
    }
}
