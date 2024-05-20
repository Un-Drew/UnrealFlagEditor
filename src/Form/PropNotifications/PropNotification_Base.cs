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
    public partial class PropNotification_Base : UserControl
    {
        public string LongerMessage;
        public TextBoxWindow OpenedWindow;

        public PropNotification_Base() : base()
        {
            InitializeComponent();

            Disposed += OnDispose;
        }

        public PropNotification_Base(string inMessage, string inLongerMessage = null) : this()
        {
            InitParams(inMessage, inLongerMessage);
        }

        public void InitParams(string inMessage, string inLongerMessage)
        {
            innerText.Text = inMessage;
            LongerMessage = inLongerMessage;

            viewLink.Visible = (LongerMessage != null);
        }

        public virtual string GetFriendlyTypeName()
        {
            return "Message";
        }

        private void viewLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (OpenedWindow == null)
            {
                OpenedWindow = new TextBoxWindow(GetFriendlyTypeName(), LongerMessage);
                OpenedWindow.FormClosed += SubWindowClosed;
                OpenedWindow.Show();
            }
            else
            {
                OpenedWindow.Activate();
            }
        }

        private void SubWindowClosed(object sender, FormClosedEventArgs e)
        {
            OpenedWindow = null;
        }

        private void OnDispose(object sender, EventArgs e)
        {
            if (OpenedWindow != null)
            {
                OpenedWindow.FormClosed -= SubWindowClosed;
                OpenedWindow.Close();
                OpenedWindow = null;
            }
        }
    }
}
