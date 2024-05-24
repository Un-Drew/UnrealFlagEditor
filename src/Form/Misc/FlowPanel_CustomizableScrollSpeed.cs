using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace UnrealFlagEditor
{
    // A way to multiply the scroll delta sent by the system.
    public class FlowPanel_CustomizableScrollSpeed : FlowLayoutPanel
    {
        protected float scrollMultiplier = 1.0f;

        [DefaultValue(1.0f)]
        public float ScrollMultiplier
        {
            get
            {
                return scrollMultiplier;
            }
            set
            {
                scrollMultiplier = value;
            }
        }

        /*
        public void PrintSizes()
        {
            Rectangle clientRectangle = base.ClientRectangle;
            Rectangle displayRect = DisplayRectangle;

            EditorForm.Print($"Properties horiz size: {clientRectangle.Width}, {displayRect.Width}, {Width}");
        }

        //
        // Summary:
        //     Raises the System.Windows.Forms.Control.Resize event.
        //
        // Parameters:
        //   e:
        //     An System.EventArgs that contains the event data.
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnResize(EventArgs e)
        {
            Rectangle clientRectangle_B = base.ClientRectangle;
            Rectangle displayRect_B = DisplayRectangle;
            float width_B = Width;

            base.OnResize(e);

            Rectangle clientRectangle_A = base.ClientRectangle;
            Rectangle displayRect_A = DisplayRectangle;
            float width_A = Width;

            EditorForm.Print($"Horizontally resized from {clientRectangle_B.Width}, {displayRect_B.Width}, {width_B} to {clientRectangle_A.Width}, {displayRect_A.Width} {width_A}");
        }
        */

        //
        // Summary:
        //     Raises the System.Windows.Forms.Control.MouseWheel event.
        //
        // Parameters:
        //   e:
        //     A System.Windows.Forms.MouseEventArgs that contains the event data.
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (e.Delta != 0 && ScrollMultiplier > 0.0f)
            {
                if (VScroll)
                {
                    Rectangle clientRectangle = base.ClientRectangle;
                    Rectangle displayRect = DisplayRectangle;
                    int num = -displayRect.Y;
                    int val = -(clientRectangle.Height - displayRect.Height);
                    int scrollAmount = (int)(e.Delta * ScrollMultiplier);
                    if (scrollAmount == 0)
                        // Don't let our multiplier reduce the scroll size to 0, it should at least be 1.
                        scrollAmount = Math.Sign(e.Delta);
                    num = Math.Max(num - scrollAmount, 0);
                    num = Math.Min(num, val);
                    AutoScrollPosition = new Point(-displayRect.X, num);
                    if (e is HandledMouseEventArgs)
                    {
                        ((HandledMouseEventArgs)e).Handled = true;
                    }
                }
                else if (HScroll)
                {
                    Rectangle clientRectangle2 = base.ClientRectangle;
                    Rectangle displayRect = DisplayRectangle;
                    int num2 = -displayRect.X;
                    int val2 = -(clientRectangle2.Width - displayRect.Width);
                    int scrollAmount = (int)(e.Delta * ScrollMultiplier);
                    if (scrollAmount == 0)
                        // Don't let our multiplier reduce the scroll size to 0, it should at least be 1.
                        scrollAmount = Math.Sign(e.Delta);
                    num2 = Math.Max(num2 - scrollAmount, 0);
                    num2 = Math.Min(num2, val2);
                    AutoScrollPosition = new Point(num2, -displayRect.Y);
                    if (e is HandledMouseEventArgs)
                    {
                        ((HandledMouseEventArgs)e).Handled = true;
                    }
                }
            }

            // Oops! Can't call base here because this is supposed to override that functionality.
            // Can't invoke the event like Control does either, because of private! Shame!
        }
    }

    // Here's a completely unrelated, fun fact: The system mouse scroll speed setting doesn't affect this kind of control whatsoever!
}
