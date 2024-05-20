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
    // UE3
    [EditorRegisterClass(typeof(UScriptStruct))]
    // UE1-2
    [EditorRegisterClass(typeof(UStruct))]
    public class EdNode_UStruct : EdNode_UObject
    {
        public override bool RequiresObjectDeserialization => true;

        public BinaryMetaData.BinaryField StructFlagsBinaryField;
        public uint StructFlags;

        public override int SortPriority => EDSRT_USTRUCT;

        static public ControlDef_Base ControlDef_Struct = new ControlDef_UStruct();

        public override void EnsureControls(PropertyPanel propPanel)
        {
            if (ControlDef_Struct.EnsureControls(propPanel))
                base.EnsureControls(propPanel);
        }

        public uint GetStructFlagsDel()
        {
            return StructFlags;
        }

        public void SetStructFlagsDel(uint inFlags)
        {
            StructFlags = inFlags;
        }

        public EdNode_UStruct(EditorEngine eng, UObject inObject) : base(eng, inObject)
        {
            ImageKey = "Struct";
            SelectedImageKey = "Struct";
        }

        public override void InitNode()
        {
            base.InitNode();
            // Only ScriptStructs (UE3 structs) are guaranteed to have this, so only error out for those.
            StructFlagsBinaryField = FindBinaryFieldByName("StructFlags", OwnerObject is UScriptStruct);
            if (StructFlagsBinaryField.Size != 0)
                StructFlags = (uint)StructFlagsBinaryField.Value;
        }

        public override bool SaveChanges()
        {
            if (!base.SaveChanges()) return false;
            WriteBinaryFieldValue_Int(StructFlagsBinaryField, StructFlags);
            return true;
        }

        public override void ApplyToDefault()
        {
            base.ApplyToDefault();
            ApplyValueToBinaryField(ref StructFlagsBinaryField, StructFlags);
        }

        public void AddStructFlag(string inIdentifier, uint flag)
        {
            Properties.Add(new EdProp_FlagUInt32(this, inIdentifier, GetStructFlagsDel, SetStructFlagsDel, flag));
        }

        public void AddStructFlag(string inIdentifier, Flags.StructFlags flag)
        {
            AddStructFlag(inIdentifier, (uint)flag);
        }

        public override void InitializeObjectProperties()
        {
            if (StructFlagsBinaryField.Size != 0)
            {
                AddStructFlag("Struct.StructFlags.Native",              Flags.StructFlags.Native);
                AddStructFlag("Struct.StructFlags.Export",              Flags.StructFlags.Export);
                if (OwnerObject.Package.Version <= 128)
                    AddStructFlag("Struct.StructFlags.Long",            Flags.StructFlags.Long);
                else
                    AddStructFlag("Struct.StructFlags.HasComponents",   Flags.StructFlags.HasComponents);
                if (OwnerObject.Package.Version < 222)
                    AddStructFlag("Struct.StructFlags.Init",            Flags.StructFlags.Init);
                else
                    AddStructFlag("Struct.StructFlags.Transient",       Flags.StructFlags.Transient);
                AddStructFlag("Struct.StructFlags.Atomic",              Flags.StructFlags.Atomic);
                AddStructFlag("Struct.StructFlags.Immutable",           Flags.StructFlags.Immutable);
                AddStructFlag("Struct.StructFlags.AtomicWhenCooked",    Flags.StructFlags.AtomicWhenCooked);
                AddStructFlag("Struct.StructFlags.ImmutableWhenCooked", Flags.StructFlags.ImmutableWhenCooked);
            }

            base.InitializeObjectProperties();
        }
    }

    public class ControlDef_UStruct : ControlDef_Base
    {
        public override void InitializeControls()
        {
            OpenCategory("Struct Flags", Color.LightCoral,
                "A set of flags specific to structs. Only officially present in UE3, but some licensed builds may have this backported to UE2."
            );

            InsertControl(new PropControl_Bool(PropPanel, "Struct.StructFlags.Native", "Native",
                "(UE2-3)\n" +
                "This struct includes native logic in C++."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Struct.StructFlags.Export", "Export",
                "(UE2-3)\n" +
                "(Only used during compiling) This struct will be included in the auto-generated C++ header."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Struct.StructFlags.Long", "Long",
                "(UE2)\n" +
                "This struct has a lot of properties, so UnrealEd shouldn't display all of them when collapsed in the property window."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Struct.StructFlags.Init", "Init",
                "(???)\n" +
                "???"
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Struct.StructFlags.HasComponents", "HasComponents",
                "(UE3)\n" +
                "This struct contains component properties."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Struct.StructFlags.Transient", "Transient",
                "(UE3)\n" +
                "Only relevant for native structs. This struct and its members should always be reconstructed via C++."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Struct.StructFlags.Atomic", "Atomic",
                "(UE3)\n" +
                "This struct should always be saved as a single unit; if any property in the struct differs from its defaults,\n"
                + "then all elements of the struct will be saved."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Struct.StructFlags.Immutable", "Immutable",
                "(UE3)\n" +
                "Requires Atomic, implied at compile-time. This struct uses binary serialization (reduces disk space and improves serialization performance).\n"
                + "It is unsafe to add/remove members from this struct without incrementing the package version."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Struct.StructFlags.StrictConfig", "StrictConfig",
                "(UE3)\n" +
                "Config properties using this struct will only save to .ini the values of the inner properties marked config.\n"
                + "Normally, config struct properties will save all of the struct's inner properties - config or not."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Struct.StructFlags.AtomicWhenCooked", "AtomicWhenCooked",
                "(UE3)\n" +
                "Only applies the Atomic flag when cooked."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "Struct.StructFlags.ImmutableWhenCooked", "ImmutableWhenCooked",
                "(UE3)\n" +
                "Requires AtomicWhenCooked, implied at compile-time. Only applies the Immutable flag when cooked."
            ));

            CloseCategory();

            base.InitializeControls();
        }
    }
}
