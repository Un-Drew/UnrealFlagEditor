using UELib.Core;

namespace UnrealFlagEditor
{
    // Special node class used for objects that don't have a matching tree node. Also shows a notification about this. Useful for debug.
    public class EdNode_Dummy : EdNode_UObject
    {
        public override int SortPriority => IsDefaultObj ? EDSRT_DEFAULTS : EDSRT_DUMMIES;

        public EdNode_Dummy(EditorEngine eng, UObject inObject) : base(eng, inObject)
        {
            ImageKey = "Unknown";
            SelectedImageKey = "Unknown";
        }

        public override void InitializeOtherProperties()
        {
            EnsureNotificationInit();
            Notifications.Add(new PropNotification_Info($"Unregistered node type: {OwnerObject.GetType()}"));

            base.InitializeOtherProperties();
        }
    }
}
