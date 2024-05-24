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
