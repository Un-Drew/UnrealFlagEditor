using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using UELib;
using UELib.Core;
using Flags = UELib.Flags;

namespace UnrealFlagEditor
{
    // This is the editor node for the UProperty class. Not to be confused with EdProp_Base.
    [EditorRegisterClass(typeof(UProperty), inPriority: EditorRegisterClassAttribute.EDPR_UPROPERTY, inAffectSubclasses: true)]
    public class EdNode_UProperty : EdNode_UObject
    {
        public override bool RequiresObjectDeserialization => true;

        public enum PropertyType
        {
            Variable = 0,
            Parameter = 1,
            Local = 2,
            Return = 3,
            Template = 4,
        }

        public PropertyType PropType;

        public BinaryMetaData.BinaryField PropFlagsBinaryField;
        public ulong PropFlags;
        public bool ArePropertyFlagsSupported = true;

        public override int SortPriority => EDSRT_UPROPERTY;

        static public ControlDef_Base ControlDef_Prop = new ControlDef_UProperty();

        public override void EnsureControls(PropertyPanel propPanel)
        {
            if (ControlDef_Prop.EnsureControls(propPanel))
                base.EnsureControls(propPanel);
        }

        public ulong GetPropertyFlagsDel()
        {
            return PropFlags;
        }

        public void SetPropertyFlagsDel(ulong inFlags)
        {
            PropFlags = inFlags;
            UpdatePropertyType();
        }

        public EdNode_UProperty(EditorEngine eng, UObject inObject) : base(eng, inObject)
        {
            ImageKey = "Variable";
            SelectedImageKey = "Variable";

            ArePropertyFlagsSupported = true;
        }

        public override void InitNode()
        {
            base.InitNode();
            //PropFlags = ((UProperty)OwnerObject).PropertyFlags;
            PropFlagsBinaryField = FindBinaryFieldByName("PropertyFlags");
            if (PropFlagsBinaryField.Size != 0)
                PropFlags = (ulong)PropFlagsBinaryField.Value;

            if (OwnerObject.Package.Build == BuildGeneration.RSS && OwnerObject.Package.LicenseeVersion >= 101)
            {
                // Batman games do something kinda weird with the property flags, effectively restricting them to a single byte.
                // I would reverse the operation on save, but I'm wondering if this is intentional in the first place?
                ArePropertyFlagsSupported = false;
                EnsureNotificationInit();
                Notifications.Add(new PropNotification_Warning(
                    "Editing PropertyFlags has been momentarily disabled for Rocksteady games, due to a lack of clarity about their custom format."
                ));
            }

            UpdatePropertyType();
        }

        public override void OnChangeMade(EdProp_Base property)
        {
            base.OnChangeMade(property);
            UpdatePropertyType();
        }

        public PropertyType DeterminePropertyType()
        {
            if ((PropFlags & (ulong)Flags.PropertyFlagsLO.ReturnParm) != 0)
            {
                return PropertyType.Return;
            }
            else if ((PropFlags & (ulong)Flags.PropertyFlagsLO.Parm) != 0)
            {
                return PropertyType.Parameter;
            }
            // This should be enough? But I'm not entirely sure...
            else if (OwnerObject.Outer is UFunction)
            {
                return PropertyType.Local;
            }
            else if (OwnerObject.Outer is UArrayProperty)
            {
                return PropertyType.Template;
            }
            else
            {
                return PropertyType.Variable;
            }
        }

        public void UpdatePropertyType()
        {
            PropType = DeterminePropertyType();

            switch (PropType)
            {
                case PropertyType.Return:
                    ImageKey = "Return";
                    break;
                case PropertyType.Parameter:
                    ImageKey = "Parameter";
                    break;
                case PropertyType.Local:
                    ImageKey = "Local";
                    break;
                case PropertyType.Template:
                    ImageKey = "Template";
                    break;
                case PropertyType.Variable:
                    ImageKey = "Variable";
                    break;
            }
            SelectedImageKey = ImageKey;
        }

        public void AddPropertyFlagProp(string identifier, ulong flagMask)
        {
            Properties.Add(new EdProp_FlagUInt64(this, identifier, GetPropertyFlagsDel, SetPropertyFlagsDel, flagMask, !ArePropertyFlagsSupported));
        }

        public void AddPropertyFlagProp(string identifier, Flags.PropertyFlagsLO flag)
        {
            AddPropertyFlagProp(identifier, (ulong)flag);
        }

        public void AddPropertyFlagProp(string identifier, Flags.PropertyFlagsHO flag)
        {
            AddPropertyFlagProp(identifier, (ulong)flag << 32);
        }

        public override bool SaveChanges()
        {
            if (!base.SaveChanges()) return false;
            WriteBinaryFieldValue_Int(PropFlagsBinaryField, PropFlags);
            return true;
        }

        public override void ApplyToDefault()
        {
            base.ApplyToDefault();
            ApplyValueToBinaryField(ref PropFlagsBinaryField, PropFlags);
        }

        public override void InitializeObjectProperties()
        {
            // I tried not to make final assumptions about which flags appear in which version, unless there's conficts in between versions.

            if (PropFlagsBinaryField.Size != 0)
            {
                ////////////////////////////////////////////
                // General
                ////////////////////////////////////////////
                AddHeaderProperty("Property.Header.General");

                AddPropertyFlagProp("Property.PropertyFlags.Const", Flags.PropertyFlagsLO.Const);
                AddPropertyFlagProp("Property.PropertyFlags.Config", Flags.PropertyFlagsLO.Config);
                AddPropertyFlagProp("Property.PropertyFlags.GlobalConfig", Flags.PropertyFlagsLO.GlobalConfig);
                AddPropertyFlagProp("Property.PropertyFlags.Localized", Flags.PropertyFlagsLO.Localized);
                AddPropertyFlagProp("Property.PropertyFlags.Travel", Flags.PropertyFlagsLO.Travel);
                AddPropertyFlagProp("Property.PropertyFlags.Input", Flags.PropertyFlagsLO.Input);
                AddPropertyFlagProp("Property.PropertyFlags.ExportObject", Flags.PropertyFlagsLO.ExportObject);
                AddPropertyFlagProp("Property.PropertyFlags.Transient", Flags.PropertyFlagsLO.Transient);
                if (OwnerObject.Package.Version > 160)
                    AddPropertyFlagProp("Property.PropertyFlags.DuplicateTransient", Flags.PropertyFlagsLO.DuplicateTransient);
                AddPropertyFlagProp("Property.PropertyFlags.Component", Flags.PropertyFlagsLO.Component);
                if (OwnerObject.Package.Version <= 300)  // Init allegedly appears in 301, which overlaps with OnDemand.
                    AddPropertyFlagProp("Property.PropertyFlags.OnDemand", Flags.PropertyFlagsLO.OnDemand);
                if (OwnerObject.Package.Version <= 160)  // DuplicateTransient allegedly appears in 161, which overlaps with New.
                    AddPropertyFlagProp("Property.PropertyFlags.New", Flags.PropertyFlagsLO.New);
                AddPropertyFlagProp("Property.PropertyFlags.NeedCtorLink", Flags.PropertyFlagsLO.NeedCtorLink);
                AddPropertyFlagProp("Property.PropertyFlags.Deprecated", Flags.PropertyFlagsLO.Deprecated);
                if (OwnerObject.Package.Version > 160)
                    AddPropertyFlagProp("Property.PropertyFlags.DataBinding", Flags.PropertyFlagsLO.DataBinding);
                AddPropertyFlagProp("Property.PropertyFlags.Interp", Flags.PropertyFlagsHO.Interp);
                AddPropertyFlagProp("Property.PropertyFlags.EditorOnly", Flags.PropertyFlagsHO.EditorOnly);
                AddPropertyFlagProp("Property.PropertyFlags.NotForConsole", Flags.PropertyFlagsHO.NotForConsole);
                AddPropertyFlagProp("Property.PropertyFlags.PrivateWrite", Flags.PropertyFlagsHO.PrivateWrite);
                AddPropertyFlagProp("Property.PropertyFlags.ProtectedWrite", Flags.PropertyFlagsHO.ProtectedWrite);
                AddPropertyFlagProp("Property.PropertyFlags.Archetype", Flags.PropertyFlagsHO.Archetype);
                AddPropertyFlagProp("Property.PropertyFlags.CrossLevelPassive", Flags.PropertyFlagsHO.CrossLevelPassive);
                AddPropertyFlagProp("Property.PropertyFlags.CrossLevelActive", Flags.PropertyFlagsHO.CrossLevelActive);

                ////////////////////////////////////////////
                // Replication
                ////////////////////////////////////////////
                AddHeaderProperty("Property.Header.Replication");

                AddPropertyFlagProp("Property.PropertyFlags.Net", Flags.PropertyFlagsLO.Net);
                AddPropertyFlagProp("Property.PropertyFlags.RepNotify", Flags.PropertyFlagsHO.RepNotify);
                AddPropertyFlagProp("Property.PropertyFlags.RepRetry", Flags.PropertyFlagsHO.RepRetry);

                ////////////////////////////////////////////
                // Native
                ////////////////////////////////////////////
                AddHeaderProperty("Property.Header.Native");

                AddPropertyFlagProp("Property.PropertyFlags.Native", Flags.PropertyFlagsLO.Native);
                if (OwnerObject.Package.Version > 300)
                    AddPropertyFlagProp("Property.PropertyFlags.Init", Flags.PropertyFlagsLO.Init);
                AddPropertyFlagProp("Property.PropertyFlags.NoExport", Flags.PropertyFlagsLO.NoExport);
                if (OwnerObject.Package.Version > 500)
                    AddPropertyFlagProp("Property.PropertyFlags.SerializeText", Flags.PropertyFlagsLO.SerializeText);

                ////////////////////////////////////////////
                // Function parameters
                ////////////////////////////////////////////
                AddHeaderProperty("Property.Header.FunctionParams");

                AddPropertyFlagProp("Property.PropertyFlags.Parm", Flags.PropertyFlagsLO.Parm);
                AddPropertyFlagProp("Property.PropertyFlags.OptionalParm", Flags.PropertyFlagsLO.OptionalParm);
                AddPropertyFlagProp("Property.PropertyFlags.OutParm", Flags.PropertyFlagsLO.OutParm);
                AddPropertyFlagProp("Property.PropertyFlags.SkipParm", Flags.PropertyFlagsLO.SkipParm);
                AddPropertyFlagProp("Property.PropertyFlags.ReturnParm", Flags.PropertyFlagsLO.ReturnParm);
                AddPropertyFlagProp("Property.PropertyFlags.CoerceParm", Flags.PropertyFlagsLO.CoerceParm);

                ////////////////////////////////////////////
                // Editor only
                ////////////////////////////////////////////
                AddHeaderProperty("Property.Header.EditorOnly");

                AddPropertyFlagProp("Property.PropertyFlags.Editable", Flags.PropertyFlagsLO.Editable);
                AddPropertyFlagProp("Property.PropertyFlags.EditConst", Flags.PropertyFlagsLO.EditConst);
                if (OwnerObject.Package.Version > 500)
                    AddPropertyFlagProp("Property.PropertyFlags.EditFixedSize", Flags.PropertyFlagsLO.EditFixedSize);
                else
                    AddPropertyFlagProp("Property.PropertyFlags.EditConstArray", Flags.PropertyFlagsLO.EditConstArray);
                if (OwnerObject.Package.Version > 160)
                {
                    AddPropertyFlagProp("Property.PropertyFlags.NoImport", Flags.PropertyFlagsLO.NoImport);
                    AddPropertyFlagProp("Property.PropertyFlags.NoClear", Flags.PropertyFlagsLO.NoClear);
                }
                else
                    AddPropertyFlagProp("Property.PropertyFlags.EditorData", Flags.PropertyFlagsLO.EditorData);
                if (OwnerObject.Package.Build != UnrealPackage.GameBuild.BuildName.AHIT)  // Overlaps with AHIT's BitWise.
                    AddPropertyFlagProp("Property.PropertyFlags.EdFindable", Flags.PropertyFlagsLO.EdFindable);
                AddPropertyFlagProp("Property.PropertyFlags.EditInline", Flags.PropertyFlagsLO.EditInline);
                AddPropertyFlagProp("Property.PropertyFlags.EditInlineUse", Flags.PropertyFlagsLO.EditInlineUse);
                if (OwnerObject.Package.Version <= 160)  // DataBinding allegedly appears in 161, which overlaps with EditInlineNotify.
                    AddPropertyFlagProp("Property.PropertyFlags.EditInlineNotify", Flags.PropertyFlagsLO.EditInlineNotify);
                AddPropertyFlagProp("Property.PropertyFlags.NonTransactional", Flags.PropertyFlagsHO.NonTransactional);
                AddPropertyFlagProp("Property.PropertyFlags.EditHide", Flags.PropertyFlagsHO.EditHide);
                AddPropertyFlagProp("Property.PropertyFlags.EditTextBox", Flags.PropertyFlagsHO.EditTextBox);

                ////////////////////////////////////////////
                // UT2004
                ////////////////////////////////////////////
                if (OwnerObject.Package.Version == 128)  // this is how UPropertyDecompiler chooses to detect it.
                {
                    AddHeaderProperty("Property.Header.UT2004");

                    AddPropertyFlagProp("Property.PropertyFlags.UT2004_Cache", Flags.PropertyFlagsLO.Cache);
                    AddPropertyFlagProp("Property.PropertyFlags.UT2004_Automated", Flags.PropertyFlagsLO.Automated);
                }

                ////////////////////////////////////////////
                // Vengeance
                ////////////////////////////////////////////
                if (OwnerObject.Package.Build == BuildGeneration.Vengeance)
                {
                    AddHeaderProperty("Property.Header.VG");

                    AddPropertyFlagProp("Property.PropertyFlags.VG_NoCheckPoint", Flags.PropertyFlagsLO.VG_NoCheckPoint);
                }

                ////////////////////////////////////////////
                // Bioshock
                ////////////////////////////////////////////
                if (OwnerObject.Package.Build == UnrealPackage.GameBuild.BuildName.Bioshock_Infinite)
                {
                    AddHeaderProperty("Property.Header.BIOSHOCK");

                    AddPropertyFlagProp("Property.PropertyFlags.BIOINF_Unk1", Flags.PropertyFlagsHO.BIOINF_Unk1);
                    AddPropertyFlagProp("Property.PropertyFlags.BIOINF_Unk2", Flags.PropertyFlagsHO.BIOINF_Unk2);
                    AddPropertyFlagProp("Property.PropertyFlags.BIOINF_Unk3", Flags.PropertyFlagsHO.BIOINF_Unk3);
                }

                ////////////////////////////////////////////
                // AHIT
                ////////////////////////////////////////////
                if (OwnerObject.Package.Build == UnrealPackage.GameBuild.BuildName.AHIT)
                {
                    AddHeaderProperty("Property.Header.AHIT");

                    AddPropertyFlagProp("Property.PropertyFlags.AHIT_Bitwise", Flags.PropertyFlagsLO.AHIT_Bitwise);
                    AddPropertyFlagProp("Property.PropertyFlags.AHIT_Serialize", Flags.PropertyFlagsHO.AHIT_Serialize);
                }
            }

            base.InitializeObjectProperties();
        }
    }

    public class ControlDef_UProperty : ControlDef_Base
    {
        public override void InitializeControls()
        {
            OpenCategory("Property Flags", Color.PaleGreen,
                "A set of flags specific to properties - that's global variables, local variables, function parameters, return values or array templates.\n" +
                "Mainly consists of keywords. Although, note that some keywords (such as private and protected) are defined in the object flags."
            );

            ////////////////////////////////////////////
            // General
            ////////////////////////////////////////////
            AddHeaderControl("Property.Header.General", "General:");

            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.Const", "Const",
                "(UE1-3)\n" +
                "This property cannot be directly modified in UnrealScript (but can still be initialized in DefaultProperties).\n"
                + "This is only enforced at compile-time and can be safely modified."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.Config", "Config",
                "(UE1-3)\n" +
                "This property's value is stored in and loaded from a .ini (config) file. Each sub-class has its own separate config value.\n"
                + "The compiler also allows directly modifying the defaults of such variables with this syntax:\n"
                + "        class'ClassName'.default.PropertyName = ???;"
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.GlobalConfig", "GlobalConfig",
                "(UE1-3)\n" +
                "Requires config, implied at compile time. Contrary to regular configs, all sub-classes share the same config value in the .ini file."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.Localized", "Localized",
                "(UE1-3)\n" +
                "This property is automatically loaded from a localization file, such as .int or .fra."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.Travel", "Travel",
                "(UE1-2)\n" +
                "This property's value is preserved between map changes - useful for singleplayer games.\n"
                + "Only affects the player Pawn, PlayerController and Inventory chain."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.Input", "Input",
                "(UE1-3)\n" +
                "Only relevant for byte and float properties. This property can be directly bound to an input button/axis (via .ini bindings)."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.ExportObject", "ExportObject",
                "(UE2-3)\n" +
                "Only relevant for object properties or arrays of them. Indicates that the object assigned to this property should be exported in its entirety\n"
                + "as a subobject block when the object is copy/pased or exported to T3D, as opposed to just outputting the object reference itself.\n"
                + "NOTE: Component properties are automatically given this flag."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.Transient", "Transient",
                "(UE1-3)\n" +
                "This property should not be loaded from or saved into packages/files."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.DuplicateTransient", "DuplicateTransient",
                "(UE3)\n" +
                "This property's value should be reset to the class default value when duplicating or copy/pasting this object."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.Component", "Component",
                "(UE3)\n" +
                "This is a component property."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.OnDemand", "OnDemand",
                "(Unknown version)\n" +
                "???"
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.New", "New",
                "(Unknown version)\n" +
                "???"
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.NeedCtorLink", "NeedCtorLink",
                "(UE1-3)\n" +
                "This property's value needs to be contructed for every instance of the parent. For object properties, this means that a\n"
                + "new object needs to be instanced for each copy of the parent (usually because the 'instanced' keyword was used).\n"
                + "NOTE: A Component property will NEVER have this enabled, because they're handled separately."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.Deprecated", "Deprecated",
                "(UE2-3)\n" +
                "This property is deprecated and should no longer be used. Its value will be ignored when saving a package/object."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.DataBinding", "DataBinding",
                "(UE3)\n" +
                "This property can be manipulated by the data store system."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.Interp", "Interp",
                "(UE3)\n" +
                "Only relevant for Float, Bool, Vector, Color and LinearColor properties. This property can be animated in Matinee."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.EditorOnly", "EditorOnly",
                "(UE3)\n" +
                "This property's value will only be loaded when running UnrealEd or a commandlet. During the game, the value for this property is discarded."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.NotForConsole", "NotForConsole",
                "(UE3)\n" +
                "This property's value will only be loaded when running on the PC. On consoles, the value for this property is discarded."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.PrivateWrite", "PrivateWrite",
                "(UDK 2009)\n" +
                "This property is const outside the class it was declared in.\n"
                + "This is only enforced at compile-time and can be safely modified."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.ProtectedWrite", "ProtectedWrite",
                "(UDK 2009)\n" +
                "This property is const outside the class it was declared in and sub-classes.\n"
                + "This is only enforced at compile-time and can be safely modified."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.Archetype", "Archetype",
                "(UE3)\n" +
                "Caused by using the 'Archetype' keyword. Effect is unknown, but supposedly should be used for properties that reference archetypes."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.CrossLevelPassive", "CrossLevelPassive",
                "(UE3)\n" +
                "Only relevant for object properties. This property can save a reference to an object in another level package.\n"
                + "The difference between Passive and Active is not yet clear."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.CrossLevelActive", "CrossLevelActive",
                "(UE3)\n" +
                "Only relevant for object properties. This property can save a reference to an object in another level package.\n"
                + "The difference between Passive and Active is not yet clear."
            ));

            ////////////////////////////////////////////
            // Replication
            ////////////////////////////////////////////
            AddHeaderControl("Property.Header.Replication", "Replication:");

            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.Net", "Net",
                "(UE1-3)\n" +
                "This property is involved with network replication (i.e. it's present in the replication block)."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.RepNotify", "RepNotify",
                "(UE3)\n" +
                "Actors should be notified (via the ReplicatedEvent function) when this value for this property is received via replication."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.RepRetry", "RepRetry",
                "(UE3)\n" +
                "Only relevant for struct properties. Retry replication of this property if it fails to be fully sent (e.g. object references\n"
                + "not yet available to serialize over the network). For simple references this is the default, but for structs this is often\n"
                + "undesirable due to the bandwidth cost, so it is disabled unless this flag is specified."
            ));

            ////////////////////////////////////////////
            // Native
            ////////////////////////////////////////////
            AddHeaderControl("Property.Header.Native", "Native:");

            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.Native", "Native",
                "(UE1-3)\n" +
                "This property is loaded and saved by C++ code, rather than by UnrealScript. Not to be confused with the native class flag."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.Init", "Init",
                "(UE3)\n" +
                "Only relevant for arrays and strings in native classes. Changes the way this property is exported to the C++ header.\n"
                + "As a consequence, default values for this kind of property are ignored when creating instances of the class."
            ));
            // TODO: Official docs say that this only affects the auto-generated headers. BeyondUnreal claims it also affects T3D,
            //       but nothing in UE3's source seems to support this. Does this vary between package versions?
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.NoExport", "NoExport",
                "(UE2-3)\n" +
                "Only relevant for native classes. This property should not be included in the auto-generated C++ header."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.SerializeText", "SerializeText",
                "(UE3)\n" +
                "Only relevant for native properties. Allows a native property to be copy/pasted with the object in UnrealEd."
            ));

            ////////////////////////////////////////////
            // Function parameters
            ////////////////////////////////////////////
            AddHeaderControl("Property.Header.FunctionParams", "Function params:");

            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.Parm", "Parm",
                "(UE1-3)\n" +
                "This property is a function parameter - unless ReturnParm is true."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.OptionalParm", "OptionalParm",
                "(UE1-3)\n" +
                "This function parameter is optional."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.OutParm", "OutParm",
                "(UE1-3)\n" +
                "This function parameter is an output, so any changes made to it will propagate to the variable passed by the caller.\n"
                + "NOTE: In UE3, this can also be used as a memory optimization for large values like arrays."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.SkipParm", "SkipParm",
                "(UE1-3)\n" +
                "Only relevant for the 2nd parameter of a native operator. This parameter can be skipped in a short-circuit fashion depending on\n" +
                "the value of the first parameter. Used for the && and || bool operators."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.ReturnParm", "ReturnParm",
                "(UE1-3)\n" +
                "This property is a return value for a function."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.CoerceParm", "CoerceParm",
                "(UE1-3)\n" +
                "Values passed to this parameter will be auto-converted by the compiler to the required type."
            ));

            ////////////////////////////////////////////
            // Editor only
            ////////////////////////////////////////////
            AddHeaderControl("Property.Header.EditorOnly", "Editor only:");

            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.Editable", "Editable",
                "(UE1-3)\n" +
                "This property is exposed in UnrealEd property windows."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.EditConst", "EditConst",
                "(UE1-3)\n" +
                "This property cannot be edited in UnrealEd property windows. Nested fields for EditInline properties can still be edited."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.EditConstArray", "EditConstArray",
                "(UE2)\n" +
                "Only relevant for dynamic arrays. This will prevent the user from changing the length of this array via the UnrealEd property window."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.EditFixedSize", "EditFixedSize",
                "(UE3)\n" +
                "Only relevant for dynamic arrays. This will prevent the user from changing the length of this array via the UnrealEd property window."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.NoImport", "NoImport",
                "(UE3)\n" +
                "This property will be skipped when pasting objects in UnrealEd."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.NoClear", "NoClear",
                "(UE3)\n" +
                "Only relevant for object properties or arrays of them. This property will not be clearable in UnrealEd property windows."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.EditorData", "EditorData",
                "(Unknown version, likely UE2.5)\n" +
                "May represent a tooltip/comment in some games. Usually in the form of a quoted string, sometimes as a double-slash comment or both."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.EdFindable", "EdFindable",
                "(UE2)\n" +
                "Adds a 'Find' button to this property in the property window, which can be used to select an actor from the UnrealEd viewports.\n"
                + "For obvious reasons this only makes sense for variables of type Actor or any of its sub-classes."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.EditInline", "EditInline",
                "(UE2-3)\n" +
                "Only relevant for object properties or arrays of them. This property can be expanded in UnrealEd property windows,\n"
                + "which will show the properties of the referenced object. This removes the ability to reference existing objects.\n"
                + "Instead, it adds a blue button to create new objects.\n"
                + "NOTE: This flag is automatically added for instanced or component properties."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.EditInlineUse", "EditInlineUse",
                "(UE2-3)\n" +
                "Requires EditinLine, implied at compile time. Contrary to regular EditinLine properties, this does not allow creating new objects\n"
                + "in UnrealEd. So this basically restores the original referencing buttons (e.g. green arrow), but still keeps the ability to expand it."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.EditInlineNotify", "EditInlineNotify",
                "(UE2)\n" +
                "Requires EditinLine, implied at compile time. Seems to provide special feedback to native code if this property is edited in a property window.\n"
                + "Seems only relevant for native classes."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.NonTransactional", "NonTransactional",
                "(UE3)\n" +
                "Changes to this property's value will not be included in UnrealEd's undo/redo history."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.EditHide", "EditHide",
                "(UE3)\n" +
                "This property is hidden in UnrealEd property windows. Allegedly, because it doesn't seem to work!"
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.EditTextBox", "EditTextBox",
                "(UE3)\n" +
                "Only relevant for string properties. This property gets an extra button in UnrealEd that opens it in a basic text editor.\n"
                + "This allows adding multiple lines to a string value, and is used by the Custom material expression for its custom HLSL code."
            ));

            ////////////////////////////////////////////
            // UT2004
            ////////////////////////////////////////////
            AddHeaderControl("Property.Header.UT2004", "UT 2004:");

            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.UT2004_Cache", "Cache",
                "(UT2004)\n" +
                "Marks properties of certain classes that should be included in UCL file entries for that class.\n"
                + "This is used e.g. for weapon and mutator names and descriptions."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.UT2004_Automated", "Automated",
                "(UT2004)\n" +
                "Only relevant for object variables in GUIMultiComponent classes. Indicates that any GUIComponent object(s) referenced by this property\n"
                + "should automatically be added to the Controls array. Also works for static and dynamic arrays. Objects that aren't GUIComponents are ignored."
            ));

            ////////////////////////////////////////////
            // Vengeance
            ////////////////////////////////////////////
            AddHeaderControl("Property.Header.VG", "Vengeance:");

            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.VG_NoCheckPoint", "NoCheckPoint",
                "(Vengeance)\n" +
                "???"
            ));

            ////////////////////////////////////////////
            // Bioshock
            ////////////////////////////////////////////
            AddHeaderControl("Property.Header.BIOSHOCK", "Bioshock:");

            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.BIOINF_Unk1", "Unknown 1",
                "(Bioshock Infinite)\n" +
                "???"
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.BIOINF_Unk2", "Unknown 2",
                "(Bioshock Infinite)\n" +
                "???"
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.BIOINF_Unk3", "Unknown 3",
                "(Bioshock Infinite)\n" +
                "???"
            ));

            ////////////////////////////////////////////
            // AHIT
            ////////////////////////////////////////////
            AddHeaderControl("Property.Header.AHIT", "A Hat in Time:");

            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.AHIT_Bitwise", "Bitwise",
                "(AHIT)\n" +
                "Only relevant for integer properties. This property should be considered as an array of bits,\n"
                + "and should be constructed as such in DefaultProperties."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Property.PropertyFlags.AHIT_Serialize", "Serialize",
                "(AHIT)\n" +
                "Only relevant for object properties or arrays of them. This property's referenced object(s)\n"
                + "should be serialized in full during file save/load."
            ));

            CloseCategory();

            base.InitializeControls();
        }
    }
}
