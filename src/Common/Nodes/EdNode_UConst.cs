using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UELib;
using UELib.Core;

namespace UnrealFlagEditor
{
    [EditorRegisterClass(typeof(UConst))]
    public class EdNode_UConst : EdNode_UObject
    {
        public override int SortPriority => EDSRT_UCONST;

        public EdNode_UConst(EditorEngine eng, UObject inObject) : base(eng, inObject)
        {
            ImageKey = "Const";
            SelectedImageKey = "Const";
        }
    }
}
