using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UELib;
using UELib.Core;

namespace UnrealFlagEditor
{
    // TreeNode for content sub-packages (groups). Not to be confused with the root package, see EdNode_RootPackage for that.
    [EditorRegisterClass(typeof(UPackage))]
    public class EdNode_UPackage : EdNode_UObject
    {
        public EdNode_UPackage(EditorEngine eng, UObject inObject) : base(eng, inObject)
        {
            ImageKey = "Package";
            SelectedImageKey = "Package";
        }
    }
}
