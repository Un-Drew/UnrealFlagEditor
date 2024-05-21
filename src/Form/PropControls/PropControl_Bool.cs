using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UnrealFlagEditor
{
    public class PropControl_Bool : CheckBox, IPropertyControl
    {
        public PropertyPanel PropPanel;

        public string MyIdentifier;
        string IPropertyControl.Identifier => MyIdentifier;

        public List<EdProp_Base> MyProps = new List<EdProp_Base>();
        List<EdProp_Base> IPropertyControl.Prop { get => MyProps; }

        IControlContainer IControlContained.ShouldNestInContainer { get; set; }

        public bool StateUpdatedFromSelf;

        public PropControl_Bool(PropertyPanel inOwner, string identifier, string Name, string toolTip = "") : base()
        {
            PropPanel = inOwner;
            ContextMenuStrip = PropPanel.PropertyContextMenu;
            MyIdentifier = identifier;
            Text = Name;
            AutoSize = true;
            if (toolTip != "")
            {
                PropPanel.PropsToolTip.SetToolTip(this, toolTip);
            }
            Font = EdProp_Base.PropertyRegularFont;
        }

        public void UpdateStateFromProps()
        {
            UpdateBold();

            StateUpdatedFromSelf = true;
            if (MyProps.Count == 0)
            {
                Enabled = false;
                CheckState = CheckState.Indeterminate;
                StateUpdatedFromSelf = false;
                return;
            }

            Enabled = ShouldAllowEdit();

            bool isChecked = ((EdProp_Checkbox)MyProps[0]).GetValue();
            foreach (EdProp_Checkbox prop in MyProps.Skip(1))
            {
                if (isChecked != prop.GetValue())
                {
                    CheckState = CheckState.Indeterminate;
                    StateUpdatedFromSelf = false;
                    return;
                }
            }

            CheckState = isChecked ? CheckState.Checked : CheckState.Unchecked;
            StateUpdatedFromSelf = false;
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            base.OnCheckStateChanged(e);

            if (StateUpdatedFromSelf) return;

            foreach (EdProp_Checkbox prop in MyProps)
            {
                prop.SetValue(Checked);
            }
            UpdateBold();
        }

        public bool IsAnyChanged()
        {
            return ((IPropertyControl)this).IsAnyChanged();
        }

        public bool ShouldAllowEdit()
        {
            foreach (EdProp_Checkbox prop in MyProps)
            {
                if (prop.DenyEdit) return false;
            }
            return true;
        }

        public void UpdateBold()
        {
            if (IsAnyChanged())
            {
                Font = EdProp_Base.PropertyModifiedFont;
            }
            else
            {
                Font = EdProp_Base.PropertyRegularFont;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (PropPanel != null)
            {
                PropPanel.PropsToolTip.SetToolTip(this, null);
            }
            ContextMenuStrip = null;
            PropPanel = null;
            MyProps.Clear();
            base.Dispose(disposing);
        }
    }
}
