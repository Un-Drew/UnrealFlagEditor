using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnrealFlagEditor
{
    public class EdProp_Checkbox : EdProp_Base
    {
        public override Type ControlType => typeof(PropControl_Bool);

        public bool OriginalValue, CurrentValue;

        public bool DenyEdit;

        public EdProp_Checkbox(EdNode_Base inOwner, string inIdentifier, bool denyEdit = false) : base(inOwner, inIdentifier)
        {
            DenyEdit = denyEdit;
        }

        public override bool IsChanged()
        {
            return OriginalValue != CurrentValue;
        }

        public override void ApplyToDefault()
        {
            OriginalValue = CurrentValue;
        }

        public bool GetValue()
        {
            return CurrentValue;
        }

        public void SetValue(bool newValue)
        {
            if (DenyEdit)
            {
                throw new HeadlessException($"Tried to modify value of {Identifier} within {OwnerNode.GetReferencePath()}," +
                    $" but the editing of this property is not allowed!"
                );
            }
            if (CurrentValue == newValue) return;
            PreValueChanged(newValue);
            CurrentValue = newValue;
            PostValueChanged();
        }

        public virtual void PreValueChanged(bool newValue)
        {
            return;
        }

        public virtual void PostValueChanged()
        {
            OwnerNode.OnChangeMade(this);
        }
    }
}
