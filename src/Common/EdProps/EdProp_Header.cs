using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnrealFlagEditor
{
    public class EdProp_Header : EdProp_Base
    {
        public override Type ControlType => typeof(PropControl_Header);

        public EdProp_Header(EdNode_Base inOwner, string inIdentifier) : base(inOwner, inIdentifier) { }
    }
}
