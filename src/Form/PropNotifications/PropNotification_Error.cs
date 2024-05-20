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
    public partial class PropNotification_Error : PropNotification_Base
    {
        public PropNotification_Error() : base()
        {
            InitializeComponent();
        }

        public PropNotification_Error(string inMessage, string inLongerMessage = null) : this()
        {
            InitParams(inMessage, inLongerMessage);
        }

        public override string GetFriendlyTypeName()
        {
            return "Exception";
        }
    }
}
