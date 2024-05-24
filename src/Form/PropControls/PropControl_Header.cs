using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace UnrealFlagEditor
{
    public class PropControl_Header : Label, IPropertyControl
    {
        public PropertyPanel PropPanel;

        public string MyIdentifier;
        string IPropertyControl.Identifier => MyIdentifier;

        List<EdProp_Base> IPropertyControl.Prop { get => null; }

        IControlContainer IControlContained.ShouldNestInContainer { get; set; }

        static public Font HeaderFont = new Font("Arial", 12.0f, FontStyle.Bold);

        public PropControl_Header(PropertyPanel inOwner, string identifier, string name) : base()
        {
            PropPanel = inOwner;
            MyIdentifier = identifier;
            Text = name;
            AutoSize = true;
            Font = HeaderFont;
            BackColor = Color.WhiteSmoke;
            BorderStyle = BorderStyle.FixedSingle;
            Padding = new Padding(4, 4, 10, 4);
            Margin = new Padding(4);
        }

        public void UpdateStateFromProps()
        {
            return;
        }

        protected override void Dispose(bool disposing)
        {
            PropPanel = null;
            base.Dispose(disposing);
        }
    }
}
