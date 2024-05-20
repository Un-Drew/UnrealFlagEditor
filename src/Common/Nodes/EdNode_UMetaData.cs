using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UELib;
using UELib.Core;

namespace UnrealFlagEditor
{
    [EditorRegisterClass(typeof(UMetaData))]
    public class EdNode_UMetaData : EdNode_UObject
    {
        public override int SortPriority => EDSRT_UMETADATA;

        public EdNode_UMetaData(EditorEngine eng, UObject inObject) : base(eng, inObject)
        {
            ImageKey = "MetaData";
            SelectedImageKey = "MetaData";
        }
    }
}
