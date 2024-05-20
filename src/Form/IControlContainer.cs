using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UnrealFlagEditor
{
    // A property panel control that contains other controls, under a specific collection.
    public interface IControlContainer : IControlContained
    {
        // Should point to this control's INTENDED collection of children. e.g. Some UserControls
        // may have multiple intermediate controls to lay things out in a certain way.
        Control.ControlCollection ContainedControls { get; }
    }

    static class IControlContainer_ExtensionMethods
    {
        static public void DetachAll(this IControlContainer container, bool recursive = true)
        {
            foreach (Control child in container.ContainedControls)
            {
                if (recursive && child is IControlContainer)
                    ((IControlContainer)child).DetachAll();
            }
            container.ContainedControls.Clear();
        }
    }
}
