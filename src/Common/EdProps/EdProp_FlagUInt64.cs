using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UnrealFlagEditor
{
    public class EdProp_FlagUInt64 : EdProp_Checkbox
    {
        private ulong FlagMask;

        public delegate ulong GetFlagsType();
        public delegate void SetFlagsType(ulong inFlags);

        private GetFlagsType GetFlags;
        private SetFlagsType SetFlags;

        public EdProp_FlagUInt64
        (
            EdNode_Base inOwner,
            string inIdentifier,
            GetFlagsType inGetFlagsDel, SetFlagsType inSetFlagsDel, ulong inFlagMask, bool inDenyEdit = false
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

            ulong flags = GetFlags();
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
