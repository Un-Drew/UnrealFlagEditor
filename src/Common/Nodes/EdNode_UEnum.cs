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
