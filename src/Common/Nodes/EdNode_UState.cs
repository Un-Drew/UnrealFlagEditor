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
    [EditorRegisterClass(typeof(UState))]
    public class EdNode_UState : EdNode_UStruct
    {
        public override bool RequiresObjectDeserialization => true;

        public BinaryMetaData.BinaryField StateFlagsBinaryField;
        public uint StateFlags;

        public override int SortPriority => EDSRT_USTATE;

        static public ControlDef_Base ControlDef_State = new ControlDef_UState();

        public override void EnsureControls(PropertyPanel propPanel)
        {
            if (ControlDef_State.EnsureControls(propPanel))
                base.EnsureControls(propPanel);
        }

        public uint GetStateFlagsDel()
        {
            return StateFlags;
        }

        public void SetStateFlagsDel(uint inFlags)
        {
            StateFlags = inFlags;
        }

        public EdNode_UState(EditorEngine eng, UObject inObject) : base(eng, inObject)
        {
            ImageKey = "State";
            SelectedImageKey = "State";
        }

        public override void InitNode()
        {
            base.InitNode();
            StateFlagsBinaryField = FindBinaryFieldByNames(new string[] { "_StateFlags", "StateFlags" }, OwnerObject.Package.Version >= 61 /* VStateFlags */);
            if (StateFlagsBinaryField.Size != 0)
                StateFlags = (uint)StateFlagsBinaryField.Value;
        }

        public override bool SaveChanges()
        {
            if (!base.SaveChanges()) return false;
            WriteBinaryFieldValue_Int(StateFlagsBinaryField, StateFlags);
            return true;
        }

        public override void ApplyToDefault()
        {
            base.ApplyToDefault();
            ApplyValueToBinaryField(ref StateFlagsBinaryField, StateFlags);
        }

        public void AddStateFlag(string inIdentifier, uint flag)
        {
            Properties.Add(new EdProp_FlagUInt32(this, inIdentifier, GetStateFlagsDel, SetStateFlagsDel, flag));
        }

        public void AddStateFlag(string inIdentifier, Flags.StateFlags flag)
        {
            AddStateFlag(inIdentifier, (uint)flag);
        }

        public override void InitializeObjectProperties()
        {
            if (StateFlagsBinaryField.Size != 0)
            {
                // It's nice not having a billion flags to keep track of, for once :P

                AddStateFlag("State.StateFlags.Editable",  Flags.StateFlags.Editable);
                AddStateFlag("State.StateFlags.Auto",      Flags.StateFlags.Auto);
                AddStateFlag("State.StateFlags.Simulated", Flags.StateFlags.Simulated);
            }

            base.InitializeObjectProperties();
        }
    }

    public class ControlDef_UState : ControlDef_Base
    {
        public override void InitializeControls()
        {
            OpenCategory("State Flags", Color.LightCyan,
                "A set of flags present in script states - or any derived types, such as classes."
            );

            InsertControl(new PropControl_Bool(PropPanel, "State.StateFlags.Editable", "Editable",
                "(UE1-3)\n" +
                "In UnrealEd, the user can change the actor's state to this."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "State.StateFlags.Auto", "Auto",
                "(UE1-3)\n" +
                "This state was marked with 'Auto', and is the default state of this actor. Calling GoToState('Auto') will redirect to this state.\n" +
                "If no state was marked as such, then the owning class must have this enabled."
            ));
            InsertControl(new PropControl_Bool(PropPanel, "State.StateFlags.Simulated", "Simulated",
                "(UE1-3)\n" +
                "This state is valid for execution on clients if the owning actor was replicated to that client, and\n"
                + "the local role of that client is either ROLE_SimulatedProxy or ROLE_DumbProxy."
            ));

            CloseCategory();

            base.InitializeControls();
        }
    }
}
