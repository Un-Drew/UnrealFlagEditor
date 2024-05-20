using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UELib;
using UELib.Core;

namespace UnrealFlagEditor
{
    [EditorRegisterClass(typeof(UEnum))]
    public class EdNode_UEnum : EdNode_UObject
    {
        public override int SortPriority => EDSRT_UENUM;

        public EdNode_UEnum(EditorEngine eng, UObject inObject) : base(eng, inObject)
        {
            ImageKey = "Enum";
            SelectedImageKey = "Enum";
        }
    }
}
