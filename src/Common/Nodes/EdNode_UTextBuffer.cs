using UELib.Core;

namespace UnrealFlagEditor
{
    [EditorRegisterClass(typeof(UTextBuffer))]
    public class EdNode_UTextBuffer : EdNode_UObject
    {
        public override int SortPriority => EDSRT_UTEXTBUFFER;

        public EdNode_UTextBuffer(EditorEngine eng, UObject inObject) : base(eng, inObject)
        {
            ImageKey = "TextBuffer";
            SelectedImageKey = "TextBuffer";
        }
    }
}
