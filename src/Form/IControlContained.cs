using System.Collections.Generic;
using System.Windows.Forms;

namespace UnrealFlagEditor
{
    // An item in a property panel container.
    public interface IControlContained
    {
        IControlContainer ShouldNestInContainer { get; set; }
    }
    
    static class IControlContained_ExtensionMethods
    {
        /**
         * Attaches this control, making sure that its nest parent (and any parents of that) are attached as well.
         * 
         * suspendedLayoutControls - Optional. If provided (not null), then the parent controls responsible for
         * containing its children will have their layout suspended and added to this list. However, this means
         * that the caller should then call ResumeLayout on the items in this list when it's done.
         */
        static public void AttachRecursive(this IControlContained c, List<Control> suspendedLayoutControls = null)
        {
            if (c.ShouldNestInContainer == null)
                return;

            if (((Control)c).Parent != null)
                return;  // Already attached.

            c.ShouldNestInContainer.AttachRecursive(suspendedLayoutControls);

            if (suspendedLayoutControls != null && c is IControlContainer)
            {
                // This exists to prevent WinForms from recalculating the category sizes with each
                // property being added. This speeds up property panel updates up to 5 times.
                Control control = ((IControlContainer)c).ContainedControls.Owner;
                control.SuspendLayout();
                suspendedLayoutControls.Add(control);
            }

            c.ShouldNestInContainer.ContainedControls.Add((Control)c);
        }
    }
}
