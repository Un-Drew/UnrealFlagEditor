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
    public partial class PropNotification_Info : PropNotification_Base
    {
        public PropNotification_Info() : base()
        {
            InitializeComponent();
        }

        public PropNotification_Info(string inMessage, string inLongerMessage = null) : this()
        {
            InitParams(inMessage, inLongerMessage);
        }

        public override string GetFriendlyTypeName()
        {
            return "Message";
        }
    }
}
