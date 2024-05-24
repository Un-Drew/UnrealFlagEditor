using System.Drawing;
using UELib;
using UELib.Core;
using Flags = UELib.Flags;

namespace UnrealFlagEditor
{
    [EditorRegisterClass(typeof(UClass))]
    public class EdNode_UClass : EdNode_UState
    {
        public override bool RequiresObjectDeserialization => true;

        public BinaryMetaData.BinaryField ClassFlagsBinaryField;
        public ulong ClassFlags;

        public override int SortPriority => EDSRT_UCLASS;
        public override bool SortAlphabetically => true;

        static public ControlDef_Base ControlDef_Class = new ControlDef_UClass();

        public override void EnsureControls(PropertyPanel propPanel)
        {
            if (ControlDef_Class.EnsureControls(propPanel))
                base.EnsureControls(propPanel);
        }

        public ulong GetClassFlagsDel()
        {
            return ClassFlags;
        }

        public void SetClassFlagsDel(ulong inFlags)
        {
            ClassFlags = inFlags;
        }

        public EdNode_UClass(EditorEngine eng, UObject inObject) : base(eng, inObject)
        {
            ImageKey = "Class";
            SelectedImageKey = "Class";
        }

        public override void InitNode()
        {
            base.InitNode();
            ClassFlagsBinaryField = FindBinaryFieldByName("ClassFlags");
            if (ClassFlagsBinaryField.Size != 0)
                ClassFlags = (ulong)ClassFlagsBinaryField.Value;
        }

        public void AddClassFlagProp(string identifier, ulong flagMask)
        {
            Properties.Add(new EdProp_FlagUInt64(this, identifier, GetClassFlagsDel, SetClassFlagsDel, flagMask));
        }

        public void AddClassFlagProp(string identifier, Flags.ClassFlags flag)
        {
            AddClassFlagProp(identifier, (ulong)flag);
        }

        public override void InitializeObjectProperties()
        {
            if (ClassFlagsBinaryField.Size != 0)
            {
                ////////////////////////////////////////////
                // General
                ////////////////////////////////////////////
                AddHeaderProperty("Class.Header.General");

                AddClassFlagProp("Class.ClassFlags.Abstract",           Flags.ClassFlags.Abstract);
                AddClassFlagProp("Class.ClassFlags.Transient",          Flags.ClassFlags.Transient);
                AddClassFlagProp("Class.ClassFlags.SafeReplace",        Flags.ClassFlags.SafeReplace);
                AddClassFlagProp("Class.ClassFlags.PerObjectConfig",    Flags.ClassFlags.PerObjectConfig);
                if (OwnerObject.Package.Version >= 300)  // ExportStructs overlaps with Interface, though it allegedly disappears in 300.
                    AddClassFlagProp("Class.ClassFlags.Interface",      0x00004000U);  // TODO: doesn't exist in ClassFlags?

                // TODO: A LOT of flags that UnrealFlags marks them as "UE3" are offset by one flag in UELib vs what's actually going on in UDK 2010 upwards.
                //       Is this a mistake, or is there actually an engine version where these flags are offset like this?

                AddClassFlagProp("Class.ClassFlags.Deprecated",         0x02000000U);
                AddClassFlagProp("Class.ClassFlags.PerObjectLocalized", 0x40000000U);  // TODO: doesn't exist in ClassFlags?

                ////////////////////////////////////////////
                // Native
                ////////////////////////////////////////////
                AddHeaderProperty("Class.Header.Native");

                AddClassFlagProp("Class.ClassFlags.Native",            0x00000080U);  // TODO: doesn't exist in ClassFlags?
                AddClassFlagProp("Class.ClassFlags.NoExport",          Flags.ClassFlags.NoExport);
                AddClassFlagProp("Class.ClassFlags.NativeReplication", Flags.ClassFlags.NativeReplication);
                if (OwnerObject.Package.Version < 300)
                    AddClassFlagProp("Class.ClassFlags.ExportStructs", Flags.ClassFlags.ExportStructs);
                AddClassFlagProp("Class.ClassFlags.Intrinsic",         0x10000000U);  // TODO: doesn't exist in ClassFlags?
                AddClassFlagProp("Class.ClassFlags.NativeOnly",        Flags.ClassFlags.NativeOnly);

                ////////////////////////////////////////////
                // Editor only
                ////////////////////////////////////////////
                AddHeaderProperty("Class.Header.EditorOnly");

                string placeableIdentifier;
                if (OwnerObject.Package.Version >= /*UField.PlaceableVersion*/ 69)
                    placeableIdentifier = "Class.ClassFlags.Placeable";
                else
                    placeableIdentifier = "Class.ClassFlags.UserCreate";
                AddClassFlagProp(placeableIdentifier,                   Flags.ClassFlags.Placeable);
                AddClassFlagProp("Class.ClassFlags.EditInlineNew",      Flags.ClassFlags.EditInlineNew);
                AddClassFlagProp("Class.ClassFlags.CollapseCategories", Flags.ClassFlags.CollapseCategories);
                AddClassFlagProp("Class.ClassFlags.Hidden",             0x01000000U);
                AddClassFlagProp("Class.ClassFlags.HideDropDown",       0x04000000U);

                ////////////////////////////////////////////
                // Auto-generated
                ////////////////////////////////////////////
                AddHeaderProperty("Class.Header.AutoGen");

                AddClassFlagProp("Class.ClassFlags.Compiled",          Flags.ClassFlags.Compiled);
                AddClassFlagProp("Class.ClassFlags.Config",            Flags.ClassFlags.Config);
                AddClassFlagProp("Class.ClassFlags.Parsed",            Flags.ClassFlags.Parsed);
                AddClassFlagProp("Class.ClassFlags.Localized",         Flags.ClassFlags.Localized);
                AddClassFlagProp("Class.ClassFlags.Instanced",         Flags.ClassFlags.Instanced);
                AddClassFlagProp("Class.ClassFlags.NeedsDefaultProps", 0x00400000U);  // TODO: doesn't exist in ClassFlags?
                AddClassFlagProp("Class.ClassFlags.HasComponents",     0x00800000U);
                AddClassFlagProp("Class.ClassFlags.Exported",          0x08000000U);
                AddClassFlagProp("Class.ClassFlags.HasCrossLevelRefs", 0x80000000U);  // TODO: doesn't exist in ClassFlags?

                ////////////////////////////////////////////
                // UT2004
                ////////////////////////////////////////////
                if (OwnerObject.Package.Build == UnrealPackage.GameBuild.BuildName.UT2004)
                {
                    AddHeaderProperty("Class.Header.UT2004");

                    AddClassFlagProp("Class.ClassFlags.UT2004_CacheExempt", Flags.ClassFlags.CacheExempt);
                }

                ////////////////////////////////////////////
                // Vengeance
                ////////////////////////////////////////////
                if (OwnerObject.Package.Build == BuildGeneration.Vengeance)
                {
                    AddHeaderProperty("Class.Header.VG");

                    AddClassFlagProp("Class.ClassFlags.VG_Interface", Flags.ClassFlags.VG_Interface);
                }

                ////////////////////////////////////////////
                // AHIT
                ////////////////////////////////////////////
                if (OwnerObject.Package.Build == UnrealPackage.GameBuild.BuildName.AHIT)
                {
                    AddHeaderProperty("Class.Header.AHIT");

                    AddClassFlagProp("Class.ClassFlags.AHIT_AlwaysLoaded",       Flags.ClassFlags.AHIT_AlwaysLoaded);
                    AddClassFlagProp("Class.ClassFlags.AHIT_IterationOptimized", Flags.ClassFlags.AHIT_IterOptimized);
                }
            }

            base.InitializeObjectProperties();
        }

        public override bool SaveChanges()
        {
            if (!base.SaveChanges()) return false;
            WriteBinaryFieldValue_Int(ClassFlagsBinaryField, ClassFlags);
            return true;
        }

        public override void ApplyToDefault()
        {
            base.ApplyToDefault();
            ApplyValueToBinaryField(ref ClassFlagsBinaryField, ClassFlags);
        }
    }

    public class ControlDef_UClass : ControlDef_Base
    {
        public override void InitializeControls()
        {
            OpenCategory("Class Flags", Color.LightSkyBlue,
                "A set of flags specific to classes. Mainly consists of the keywords added in the class header in UnrealScript."
            );

            ////////////////////////////////////////////
            // General
            ////////////////////////////////////////////
            AddHeaderControl("Class.Header.General", "General:");

            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.Abstract", "Abstract",
                "(UE1-3)\n" +
                "This class cannot be directly instantiated."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.Transient", "Transient",
                "(UE1-3)\n" +
                "Objects of this class cannot be saved in package files. (Might affect BasicSaveObject too?)"
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.SafeReplace", "SafeReplace",
                "(UE1-2)\n" +
                "Instances of this class can safely be replaced by some kind of default instance if a specific instance is unavailable."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.PerObjectConfig", "PerObjectConfig",
                "(UE1-3)\n" +
                "Each instance of this class has its own header in the config files."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.Interface", "Interface",
                "(UE3)\n" +
                "This class is an interface."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.Deprecated", "Deprecated",
                "(UE3)\n" +
                "This class is deprecated and should be removed."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.PerObjectLocalized", "PerObjectLocalized",
                "(UE3)\n" +
                "Each instance of this class has its own header in the localization files."
            ));

            ////////////////////////////////////////////
            // Native
            ////////////////////////////////////////////
            AddHeaderControl("Class.Header.Native", "Native:");

            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.Native", "Native",
                "(UE3?)\n" +
                "This class should be mirrored in C++, so it can use native code.\n"
                + "NOTE: In older versions of unreal, you might find this in the object flags instead."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.NoExport", "NoExport",
                "(UE1-3)\n" +
                "(Only used during compiling) This native class shouldn't have its C++ headers auto-generated by the compiler."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.NativeReplication", "NativeReplication",
                "(UE1-3)\n" +
                "Replication of variable values for this class is handled in the C++ implementation."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.ExportStructs", "ExportStructs",
                "(UE2-3)\n" +
                "Only used during compiling. All structs declared in this non-native class should be exported to native header files."
                + " Structs declared in native classes or with the native modifier are already exported without this modifier."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.Intrinsic", "Intrinsic",
                "(UE3?)\n" +
                "This native class shouldn't have a .uc mirror. You'll probably never find this flag enabled in any package, considering such classes are never saved."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.NativeOnly", "NativeOnly",
                "(UE3)\n" +
                "Properties of this class can only be used by native code."
            ));

            ////////////////////////////////////////////
            // Editor only
            ////////////////////////////////////////////
            AddHeaderControl("Class.Header.EditorOnly", "Editor only:");

            // TODO: This description might be wrong. UE1 source calls it NoUserCreate.
            //       I have no way to test this ATM, so if anyone has a UE1 package that's *this* old, let me know.
            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.UserCreate", "UserCreate",
                "(UE1)\n" +
                "This class can be placed in UnrealEd. NOTE: This was not tested - the logic of this may be flipped?"
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.Placeable", "Placeable",
                "(UE2-3)\n" +
                "This class can be placed in UnrealEd."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.EditInlineNew", "EditInlineNew",
                "(UE2-3)\n" +
                "Objects of this class can be created from the UnrealEd property window."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.CollapseCategories", "CollapseCategories",
                "(UE2-3)\n" +
                "Properties of this class should not be grouped in categories in UnrealEd property windows."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.Hidden", "Hidden",
                "(UE3?)\n" +
                "Seemingly unused. Hides this class in some situations, such as the kismet search."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.HideDropDown", "HideDropDown",
                "(UE2-3)\n" +
                "Prevents this class from showing up in UnrealEd property window combo boxes."
            ));

            ////////////////////////////////////////////
            // Auto-generated
            ////////////////////////////////////////////
            AddHeaderControl("Class.Header.AutoGen", "Auto-generated:");

            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.Compiled", "Compiled",
                "(UE1-3)\n" +
                "This class was compiled successfully."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.Config", "Config",
                "(UE1-3)\n" +
                "This class contains or inherits config properties, which means that it should save/load them from a config file."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.Parsed", "Parsed",
                "(UE1-3)\n" +
                "Only used during compiling. This class was parsed by the compiler."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.Localized", "Localized",
                "(UE1-3)\n" +
                "This class contains or inherits localized properties (directly, or nested in a struct/array).\n"
                + "According to BeyondUnreal, some older versions of UE1 require this to be specified manually."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.Instanced", "HasInstancedProps",
                "(UE3?)\n" +
                "This class contains or inherits instanced object properties (directly, or nested in a struct/array).\n"
                + "This includes properties with the 'Instanced' or 'EditInline Export' keywords, but always excludes Components."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.NeedsDefaultProps", "NeedsDefaultProps",
                "(UE3?)\n" +
                "Only used during compiling. Marks that this class needs its defaultproperties imported. You'll probably never find this enabled."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.HasComponents", "HasComponents",
                "(UE3)\n" +
                "This class contains or inherits component properties (directly, or nested in a struct/array)."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.Exported", "Exported",
                "(UE3?)\n" +
                "Only used during compiling. This class's C++ header was exported."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.HasCrossLevelRefs", "HasCrossLevelRefs",
                "(UE3)\n" +
                "This class contains or inherits CrossLevelPasssive or CrossLevelActive properties."
            ));

            // NOTE: Never, ever, should this method check the package's build or version, as these controls persist past package reloads.

            ////////////////////////////////////////////
            // UT2004
            ////////////////////////////////////////////
            AddHeaderControl("Class.Header.UT2004", "UT 2004:");

            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.UT2004_CacheExempt", "CacheExempt",
                "(UT2004)\n" +
                "This class should not be exported to UCL files. Only has an effect for classes that are exported by default."
            ));

            ////////////////////////////////////////////
            // Vengeance
            ////////////////////////////////////////////
            AddHeaderControl("Class.Header.VG", "Vengeance:");

            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.VG_Interface", "Interface",
                "(Vengeance)\n" +
                "This class is an interface."
            ));

            ////////////////////////////////////////////
            // AHIT
            ////////////////////////////////////////////
            AddHeaderControl("Class.Header.AHIT", "A Hat in Time:");

            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.AHIT_AlwaysLoaded", "AlwaysLoaded",
                "(AHIT)\n" +
                "This class (or one of its parents) is manually marked as AlwaysLoaded, meaning that it, and its referenced objects,\n"
                + "will be cooked in the script package."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Class.ClassFlags.AHIT_IterationOptimized", "IterationOptimized",
                "(AHIT)\n" +
                "This class has the IterationOptimized tag, meaning that actor iterators will cache their results for this type."
            ));

            CloseCategory();

            base.InitializeControls();
        }
    }
}
