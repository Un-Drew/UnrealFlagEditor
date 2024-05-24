using System;
using System.Drawing;
using System.IO;
using System.Linq;
using UELib;
using UELib.Core;
using UELib.Engine;
using Flags = UELib.Flags;

namespace UnrealFlagEditor
{
    [EditorRegisterClass(typeof(UObject), inPriority: EditorRegisterClassAttribute.EDPR_UOBJECT)]
    [EditorRegisterClass(typeof(UnknownObject), inPriority: EditorRegisterClassAttribute.EDPR_UOBJECT)]
    [EditorRegisterClass(typeof(AActor), inPriority: EditorRegisterClassAttribute.EDPR_UOBJECT, inAffectSubclasses: true)]
    [EditorRegisterClass(typeof(UComponent), inPriority: EditorRegisterClassAttribute.EDPR_UOBJECT, inAffectSubclasses: true)]
    public class EdNode_UObject : EdNode_Base
    {
        public UObject OwnerObject;

        public ulong OldObjectFlags, ObjectFlags;
        public long ObjectFlagsPos;
        public bool AreObjectFlagsSupported;

        public bool IsDefaultObj;

        public virtual bool RequiresObjectDeserialization => false;

        public override int SortPriority => IsDefaultObj ? EDSRT_DEFAULTS : EDSRT_UOBJECT;
        public override bool SortAlphabetically => IsDefaultObj;

        static public ControlDef_Base ControlDef_Obj = new ControlDef_UObject();

        public override void EnsureControls(PropertyPanel propPanel)
        {
            if (ControlDef_Obj.EnsureControls(propPanel))
                base.EnsureControls(propPanel);
        }

        public ulong GetObjFlagsDel()
        {
            return ObjectFlags;
        }

        public void SetObjFlagsDel(ulong inFlags)
        {
            ObjectFlags = inFlags;
        }

        public EdNode_UObject(EditorEngine eng, UObject inObject) : base(eng, inObject)
        {
            OwnerObject = inObject;
            Text = OwnerObject.Name;
            Name = Text.ToUpperInvariant();  // For searches.

            // This is used by sorting, so it must be set before that.
            // Fortunately, this only relies on the export table, which is already deserialized by now.
            IsDefaultObj = IsDefaultObject();

            ImageKey = "Object";
            SelectedImageKey = "Object";
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                // Free any other managed objects here
            }

            // Free any unmanaged objects here. 
            OwnerObject = null;
        }

        static public string GetPathWithPackage(UObject o)
        {
            return $"{o.Package.PackageName}.{o.GetPath()}";
        }

        public override string GetPropertyPanelName()
        {
            return OwnerObject.GetReferencePath();
        }

        public override string GetReferencePath()
        {
            return OwnerObject.Class != null
                ? $"{OwnerObject.Class.Name}'{GetPathWithPackage(OwnerObject)}'"
                : $"Class'{GetPathWithPackage(OwnerObject)}'";
        }

        // Returned string should be in invariant-uppercase so it's easier to compare.
        public override string GetUnrealTypeString()
        {
            UObject cls = OwnerObject.Class;
            if (cls == null)  // It's safe to make this assumption since all UObject nodes are export objects.
                return "CLASS";
            else
                return cls.Name.ToUpperInvariant();
        }

        public override void InitNode()
        {
            base.InitNode();

            if (RequiresObjectDeserialization
#if !ALWAYS_ONLY_DESERIALIZE_ON_DEMAND
                && OwnerObject.ShouldDeserializeOnDemand
#endif
                && OwnerObject.DeserializationState == 0
            )
            {
                OwnerObject.BeginDeserializing();
            }

            if ((OwnerObject.DeserializationState & UObject.ObjectState.Errorlized) != 0)
            {
                ErrorFlags |= NodeErrorFlags.ObjectDeserializationException;
                EnsureNotificationInit();
                // TODO: Figure out if textboxes (what error message windows use) are hardcoded to use CRLF,
                // or if it's system dependent. Would a linux user ever need to use this? IDK.
                Notifications.Add(new PropNotification_Error("Deserialization exception:",
                    $"At {OwnerObject.ExceptionPosition}/{OwnerObject.ExportTable.SerialSize} in object buffer.{Environment.NewLine}{OwnerObject.ThrownException}"
                    + $"{Environment.NewLine}This package might not be fully supported by UELib."
                ));
            }

            ObjectFlags = OwnerObject.ExportTable.ObjectFlags;
            OldObjectFlags = ObjectFlags;
        }

        public bool IsDefaultObject()
        {
            return OwnerObject.HasObjectFlag(Flags.ObjectFlagsHO.PropertiesObject)
                    // "Just in-case we have passed an overlapped object flag in UE2 or older packages." -UObject
                    && OwnerObject.Package.Version >= (uint)UELib.Branch.PackageObjectLegacyVersion.ClassDefaultCheckAddedToTemplateName;
        }

        // UELib does record the position of ObjectFlags, but the field was marked obsolete, so I'd rather not touch it.
        static public long FindObjectFlagsBytePosition(IUnrealStream stream, UExportTableItem exp)
        {
            stream.Position = exp.Offset;

            stream.ReadIndex();  // Skip Class
            stream.ReadIndex();  // Skip Super
            stream.Skip(4);      // Skip Outer

            if (stream.Package.Build == UnrealPackage.GameBuild.BuildName.BioShock &&
                stream.Version >= 132)
            {
                stream.Skip(sizeof(int));  // Skip whatever this is.
            }

            stream.ReadNameReference();  // Skip Name
            if (stream.Version >= 220 /* UExportTableItem.VArchetype */ )
            {
                stream.Skip(4);  // Skip Archetype
            }

            if (stream.Package.Build == BuildGeneration.RSS)
            {
                stream.Skip(sizeof(int));  // Skip whatever this is.
            }

            long objectFlagsPosition = stream.Position;
            ulong allegedlyObjectFlags = ReadObjectFlags(stream, exp);

            if (allegedlyObjectFlags != exp.ObjectFlags)
            {
                throw new DeserializationException($"Could not find ObjectFlags at expected position in export entry of {exp.GetReferencePath()}");
            }
            return objectFlagsPosition;
        }

        static public ulong ReadObjectFlags(IUnrealStream stream, UExportTableItem exp)
        {
            if (stream.Package.Build == UnrealPackage.GameBuild.BuildName.BioShock &&
                stream.LicenseeVersion >= 40)
            {
                // Like UE3 but without the shifting of flags
                return stream.ReadUInt64();
            }
            else
            {
                ulong objectFlags = stream.ReadUInt32();
                if (stream.Version >= UExportTableItem.VObjectFlagsToULONG)
                {
                    objectFlags = (objectFlags << 32) | stream.ReadUInt32();
                }
                return objectFlags;
            }
        }

        static public void WriteObjectFlags(IUnrealStream stream, UExportTableItem exp, ulong flags)
        {
            if (stream.Package.Build == UnrealPackage.GameBuild.BuildName.BioShock &&
                stream.LicenseeVersion >= 40)
            {
                // Like UE3 but without the shifting of flags
                Write(stream, flags);
            }
            else
            {
                if (stream.Version >= UExportTableItem.VObjectFlagsToULONG)
                {
                    Write(stream, (uint)(flags >> 32));
                }
                Write(stream, (uint)flags);
            }
        }

        public bool IsSavingExportTableSupported()
        {
            // Don't support other engine branches, for now.
            return OwnerObject.Package.Branch.Serializer != null && OwnerObject.Package.Branch.Serializer is UELib.Branch.DefaultPackageSerializer;
        }

        public override bool SaveChanges()
        {
            if (AreObjectFlagsSupported)
            {
                OwnerObject.Package.Stream.Seek(ObjectFlagsPos, SeekOrigin.Begin);
                WriteObjectFlags(OwnerObject.Package.Stream, OwnerObject.ExportTable, ObjectFlags);
            }
            return true;
        }

        public override void ApplyToDefault()
        {
            base.ApplyToDefault();
            OldObjectFlags = ObjectFlags;
        }

        public BinaryMetaData.BinaryField FindBinaryFieldByName(string name, bool errorIfNotFound = true)
        {
            if (OwnerObject.BinaryMetaData != null)
            {
                foreach (var binaryField in OwnerObject.BinaryMetaData.Fields)
                {
                    if (string.Equals(binaryField.Field, name))
                    {
                        return binaryField;
                    }
                }
            }

            if (errorIfNotFound)
            {
                EnsureNotificationInit();
                Notifications.Add(new PropNotification_Error($"Couldn't find recorded property {name}!"));
            }

            return new BinaryMetaData.BinaryField();
        }

        public BinaryMetaData.BinaryField FindBinaryFieldByNames(string[] names, bool errorIfNotFound = true)
        {
            if (OwnerObject.BinaryMetaData != null)
            {
                foreach (var binaryField in OwnerObject.BinaryMetaData.Fields)
                {
                    if (names.Contains(binaryField.Field))
                    {
                        return binaryField;
                    }
                }
            }

            if (errorIfNotFound)
            {
                EnsureNotificationInit();
                Notifications.Add(new PropNotification_Error($"Couldn't find recorded property with one of these names: {string.Join(", ", names)}!"));
            }

            return new BinaryMetaData.BinaryField();
        }

        public bool WriteBinaryFieldValue_Int(BinaryMetaData.BinaryField binaryField, object value)
        {
            if (binaryField.Size == 0) return false;

            // Depending on the engine version, the size of a field might vary, so handle that here on write.
            // Note: Using Convert.To______ because trying to cast an object to (uint) or any other numeric type
            // causes an exception, if it isn't EXACTLY of that type.
            if (binaryField.Size == 8)
                return WriteBinaryFieldValue_Int64(binaryField, Convert.ToUInt64(value));
            else if (binaryField.Size == 4)
                return WriteBinaryFieldValue_Int32(binaryField, Convert.ToUInt32(value));
            else if (binaryField.Size == 2)
                return WriteBinaryFieldValue_Int16(binaryField, Convert.ToUInt16(value));
            else if (binaryField.Size == 1)
                return WriteBinaryFieldValue_Int8(binaryField, Convert.ToByte(value));
            else
                throw new ArgumentException($"Unexpected field size {binaryField.Size} while writing int {binaryField.Field}");
        }

        // Welcome to copy-paste town.
        public bool WriteBinaryFieldValue_Int8(BinaryMetaData.BinaryField binaryField, byte value)
        {
            if (Convert.ToByte(binaryField.Value) == value) return false;

            IUnrealStream stream = OwnerObject.Package.Stream;
            stream.Seek(OwnerObject.ExportTable.SerialOffset + binaryField.Offset, SeekOrigin.Begin);
            Write(stream, value);

            return true;
        }

        public bool WriteBinaryFieldValue_Int16(BinaryMetaData.BinaryField binaryField, ushort value)
        {
            if (Convert.ToUInt16(binaryField.Value) == value) return false;

            IUnrealStream stream = OwnerObject.Package.Stream;
            stream.Seek(OwnerObject.ExportTable.SerialOffset + binaryField.Offset, SeekOrigin.Begin);
            Write(stream, value);

            return true;
        }

        public bool WriteBinaryFieldValue_Int32(BinaryMetaData.BinaryField binaryField, uint value)
        {
            if (Convert.ToUInt32(binaryField.Value) == value) return false;

            IUnrealStream stream = OwnerObject.Package.Stream;
            stream.Seek(OwnerObject.ExportTable.SerialOffset + binaryField.Offset, SeekOrigin.Begin);
            Write(stream, value);

            return true;
        }

        public bool WriteBinaryFieldValue_Int64(BinaryMetaData.BinaryField binaryField, ulong value)
        {
            if (Convert.ToUInt64(binaryField.Value) == value) return false;

            IUnrealStream stream = OwnerObject.Package.Stream;
            stream.Seek(OwnerObject.ExportTable.SerialOffset + binaryField.Offset, SeekOrigin.Begin);
            Write(stream, value);

            return true;
        }

        public void ApplyValueToBinaryField(ref BinaryMetaData.BinaryField binaryField, object value)
        {
            if (binaryField.Size == 0) return;
            binaryField.Value = value;
        }

        public void AddObjectFlagProp(string identifier, ulong flagMask)
        {
            Properties.Add(new EdProp_FlagUInt64(this, identifier, GetObjFlagsDel, SetObjFlagsDel, flagMask, !AreObjectFlagsSupported));
        }

        public void AddObjectFlagProp(string identifier, Flags.ObjectFlagsLO flag)
        {
            AddObjectFlagProp(identifier, (ulong)flag);
        }

        public void AddObjectFlagProp(string identifier, Flags.ObjectFlagsHO flag)
        {
            AddObjectFlagProp(identifier, (ulong)flag << 32);
        }

        public override void InitializeExportTableProperties()
        {
            AreObjectFlagsSupported = IsSavingExportTableSupported();
            if (AreObjectFlagsSupported)
            {
                try
                {
                    ObjectFlagsPos = FindObjectFlagsBytePosition(OwnerObject.Package.Stream, OwnerObject.ExportTable);
                }
                catch (Exception e)
                {
                    AreObjectFlagsSupported = false;
                    EnsureNotificationInit();
                    Notifications.Add(new PropNotification_Error("Position lookup exception - Changes to ObjectFlags have been disabled.", e.ToString()));
                }
            }
            else
            {
                Notifications.Add(new PropNotification_Warning("Changes to ObjectFlags aren't supported for this package, as it uses an unsupported branch."));
            }

            ////////////////////////////////////////////
            // General
            ////////////////////////////////////////////
            AddHeaderProperty("Object.ObjectFlags.Header.General");

            AddObjectFlagProp("Object.ObjectFlags.Transactional",          Flags.ObjectFlagsLO.Transactional);
            if (OwnerObject.Package.Version >= (uint)UELib.Branch.PackageObjectLegacyVersion.UE3)
                AddObjectFlagProp("Object.ObjectFlags.Obsolete",           0x0000000000000020UL);  // Accidently in HO?
            else
                AddObjectFlagProp("Object.ObjectFlags.SourceModified",     0x0000000000000020UL);
            AddObjectFlagProp("Object.ObjectFlags.Public",                 Flags.ObjectFlagsLO.Public);
            if (OwnerObject.Package.Version >= UExportTableItem.VObjectFlagsToULONG)
                AddObjectFlagProp("Object.ObjectFlags.Protected",          Flags.ObjectFlagsHO.Protected);  // in the high half of ulong
            else
                // TODO: Which engine version uses this position of Protected? It must be here in LO for a reason, right?
                AddObjectFlagProp("Object.ObjectFlags.Protected",          Flags.ObjectFlagsLO.Protected);  // in the low half of ulong
            AddObjectFlagProp("Object.ObjectFlags.Private",                Flags.ObjectFlagsLO.Private);
            if (OwnerObject.Package.Version != 128)  // Overlaps with (UT2004's?) Automated.
                AddObjectFlagProp("Object.ObjectFlags.PerObjectLocalized", 0x0000000000000100UL);  // Accidently in HO?
            AddObjectFlagProp("Object.ObjectFlags.Standalone",             Flags.ObjectFlagsLO.Standalone);
            AddObjectFlagProp("Object.ObjectFlags.HasStack",               Flags.ObjectFlagsLO.HasStack);
            AddObjectFlagProp("Object.ObjectFlags.Native",                 Flags.ObjectFlagsLO.Native);
            AddObjectFlagProp("Object.ObjectFlags.Final",                  Flags.ObjectFlagsHO.Final);
            AddObjectFlagProp("Object.ObjectFlags.DefaultObject",          Flags.ObjectFlagsHO.PropertiesObject);
            AddObjectFlagProp("Object.ObjectFlags.ArchetypeObject",        Flags.ObjectFlagsHO.ArchetypeObject);
            AddObjectFlagProp("Object.ObjectFlags.RemappedName",           Flags.ObjectFlagsHO.RemappedName);
            AddObjectFlagProp("Object.ObjectFlags.LocalizedResource",      0x0008000000000000UL);

            ////////////////////////////////////////////
            // Load rules
            ////////////////////////////////////////////
            AddHeaderProperty("Object.ObjectFlags.Header.LoadRules");

            AddObjectFlagProp("Object.ObjectFlags.LoadForClient", Flags.ObjectFlagsLO.LoadForClient);
            AddObjectFlagProp("Object.ObjectFlags.LoadForServer", Flags.ObjectFlagsLO.LoadForServer);
            AddObjectFlagProp("Object.ObjectFlags.LoadForEdit",   Flags.ObjectFlagsLO.LoadForEdit);
            AddObjectFlagProp("Object.ObjectFlags.NotForClient",  Flags.ObjectFlagsLO.NotForClient);
            AddObjectFlagProp("Object.ObjectFlags.NotForServer",  Flags.ObjectFlagsLO.NotForServer);
            AddObjectFlagProp("Object.ObjectFlags.NotForEdit",    Flags.ObjectFlagsLO.NotForEdit);

            ////////////////////////////////////////////
            // UT2004
            ////////////////////////////////////////////
            if (OwnerObject.Package.Version == 128)  // this is how UPropertyDecompiler chooses to detect it.
            {
                AddHeaderProperty("Object.ObjectFlags.Header.UT2004");

                // TODO: Making a huge assumption here, based on the flags of other types like UProperty. Could be wrong?
                AddObjectFlagProp("Object.ObjectFlags.UT2004_Automated", Flags.ObjectFlagsLO.Automated);
            }

            ////////////////////////////////////////////
            // Vengeance
            ////////////////////////////////////////////
            if (OwnerObject.Package.Build == BuildGeneration.Vengeance)
            {
                AddHeaderProperty("Object.ObjectFlags.Header.Vengeance");

                AddObjectFlagProp("Object.ObjectFlags.VG_Unnamed", Flags.ObjectFlagsLO.VG_Unnamed);
            }

            base.InitializeExportTableProperties();
        }
    }

    public class ControlDef_UObject : ControlDef_Base
    {
        public override void InitializeControls()
        {
            OpenCategory("Object Flags", Color.LightGray,
                "Any object in a package has this set of generic flags."
            );

            ////////////////////////////////////////////
            // General
            ////////////////////////////////////////////
            AddHeaderControl("Object.ObjectFlags.Header.General", "General:");

            InsertControl(new PropControl_Bool(PropPanel, "Object.ObjectFlags.Transactional", "Transactional",
                "(UE1-3)\n" +
                "This object can be recorded in the undo-redo history. Usually only used in content or map packages."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Object.ObjectFlags.SourceModified", "SourceModified",
                "(UE1?)\n" +
                "???"
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Object.ObjectFlags.Obsolete", "Obsolete",
                "(UE3)\n" +
                "Seemingly unused?"
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Object.ObjectFlags.Public", "Public",
                "(UE1-3)\n" +
                "This object is visible for other packages and can be imported by them.\n"
                + "For UE3 properties, this determines whether or not it was compiled with the 'private' keyword.\n"
                + "If this is disabled, the compiler will only allow the owner class to reference it."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Object.ObjectFlags.Protected", "Protected",
                "(UE3, UE2?)\n" +
                "Only relevant for properties. Requires Public. This determines whether or not this property was compiled with the 'protected' keyword.\n"
                + "If this is enabled, the compiler will only allow the owner class or its sub-classes to reference it."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Object.ObjectFlags.Private", "Private",
                "(UE2, removed in UE3 - see Public above)\n" +
                "Only relevant for properties. This determines whether or not this property was compiled with the 'private' keyword.\n"
                + "If this is enabled, the compiler will only allow the owner class to reference it.\n"
                + "NOTE: If this is enabled, then Public should be disabled!"
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Object.ObjectFlags.PerObjectLocalized", "PerObjectLocalized",
                "(UE3)\n" +
                "This object is an instance of a PerObjectLocalized class."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Object.ObjectFlags.Standalone", "Standalone",
                "(UE1-3)\n" +
                "This object should be kept around for editing, even if unreferenced. e.g. Classes or content assets."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Object.ObjectFlags.HasStack", "HasStack",
                "(UE1-3)\n" +
                "This object has information about its current script state. Usually only relevant for Actor instances in map packages."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Object.ObjectFlags.Native", "Native",
                "(UE1-3)\n" +
                "Only relevant for classes. This is a native class."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Object.ObjectFlags.Final", "Final",
                "(UE3?)\n" +
                "???"
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Object.ObjectFlags.DefaultObject", "DefaultObject",
                "(UE3)\n" +
                "This object is this class's default template."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Object.ObjectFlags.ArchetypeObject", "ArchetypeObject",
                "(UE3)\n" +
                "This object is an archetype template (non-default)."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Object.ObjectFlags.RemappedName", "RemappedName",
                "(UE3?)\n" +
                "???"
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Object.ObjectFlags.LocalizedResource", "LocalizedResource",
                "(UE3)\n" +
                "Only relevant for cooked packages. This object is a localized asset - excluding those in _LOC_INT packages."
            ));

            ////////////////////////////////////////////
            // Load rules
            ////////////////////////////////////////////
            AddHeaderControl("Object.ObjectFlags.Header.LoadRules", "Load rules:");

            InsertControl(new PropControl_Bool(PropPanel, "Object.ObjectFlags.LoadForClient", "LoadForClient",
                "(UE1-3)\n" +
                "This object should, at least, be loaded for clients."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Object.ObjectFlags.LoadForServer", "LoadForServer",
                "(UE1-3)\n" +
                "This object should, at least, be loaded for the server."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Object.ObjectFlags.LoadForEdit", "LoadForEdit",
                "(UE1-3)\n" +
                "This object should, at least, be loaded for the editor."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Object.ObjectFlags.NotForClient", "NotForClient",
                "(UE1-3)\n" +
                "Loading this object should be skipped for clients."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Object.ObjectFlags.NotForServer", "NotForServer",
                "(UE1-3)\n" +
                "Loading this object should be skipped for the server."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Object.ObjectFlags.NotForEdit", "NotForEdit",
                "(UE1-3)\n" +
                "Loading this object should be skipped for the editor."
            ));

            ////////////////////////////////////////////
            // UT2004
            ////////////////////////////////////////////
            AddHeaderControl("Object.ObjectFlags.Header.UT2004", "UT 2004:");

            InsertControl(new PropControl_Bool(PropPanel, "Object.ObjectFlags.UT2004_Automated", "Automated",
                "(UT2004?)\n" +
                "???"
            ));

            ////////////////////////////////////////////
            // Vengeance
            ////////////////////////////////////////////
            AddHeaderControl("Object.ObjectFlags.Header.Vengeance", "Vengeance:");

            InsertControl(new PropControl_Bool(PropPanel, "Object.ObjectFlags.VG_Unnamed", "Unnamed",
                "(Vengeance)\n" +
                "???"
            ));

            CloseCategory();

            base.InitializeControls();
        }
    }
}
