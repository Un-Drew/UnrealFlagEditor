using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UnrealFlagEditor
{
    public partial class PropContainer_Category : UserControl, IControlContainer
    {
        IControlContainer IControlContained.ShouldNestInContainer { get; set; }
        ControlCollection IControlContainer.ContainedControls => flowPanel.Controls;

        public PropertyPanel PropPanel;

        public PropContainer_Category() : base()
        {
            InitializeComponent();
            Disposed += OnDispose;
        }

        public PropContainer_Category(PropertyPanel inForm, string name, Color bgColor, string inToolTip = null) : this()
        {
            PropPanel = inForm;

            groupBox.Text = name;
            BackColor = bgColor;

            if (inToolTip != null)
                PropPanel.PropsToolTip.SetToolTip(groupBox, inToolTip);
        }

        public void UpdateStateFromProps()
        {
            return;
        }

        protected void OnDispose(object sender, EventArgs e)
        {
            ((IControlContained)this).ShouldNestInContainer = null;

            if (PropPanel != null)
            {
                PropPanel.PropsToolTip.SetToolTip(groupBox, null);
            }
            PropPanel = null;
        }
    }
}
