namespace UnrealFlagEditor
{
    public class EdProp_FlagUInt32 : EdProp_Checkbox
    {
        private uint FlagMask;

        public delegate uint GetFlagsType();
        public delegate void SetFlagsType(uint inFlags);

        private GetFlagsType GetFlags;
        private SetFlagsType SetFlags;

        public EdProp_FlagUInt32
        (
            EdNode_Base inOwner,
            string inIdentifier,
            GetFlagsType inGetFlagsDel, SetFlagsType inSetFlagsDel, uint inFlagMask, bool inDenyEdit = false
        ) : base(inOwner, inIdentifier, inDenyEdit)
        {
            FlagMask = inFlagMask;
            GetFlags = inGetFlagsDel;
            SetFlags = inSetFlagsDel;
            OriginalValue = ((GetFlags() & FlagMask) != 0);
            CurrentValue = OriginalValue;
        }

        public override void PreValueChanged(bool newValue)
        {
            base.PreValueChanged(newValue);

            uint flags = GetFlags();
            if (newValue)
                flags |= FlagMask;
            else
                flags &= ~FlagMask;
            SetFlags(flags);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                // Free any other managed objects here
            }

            // Free any unmanaged objects here. 
            GetFlags = null;
            SetFlags = null;
        }
    }
}
