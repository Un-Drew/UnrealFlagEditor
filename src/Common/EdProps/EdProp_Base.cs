using System;
using System.Collections.Generic;
using System.Drawing;

namespace UnrealFlagEditor
{
    // A property within a class that can be controlled with an IPropertyControl. Not to be confused with EdNode_UProperty.
    public class EdProp_Base : IDisposable
    {
        public EdNode_Base OwnerNode;
        public string Identifier;
        // Cached control that corresponds to this property.
        public IPropertyControl Control;

        public virtual Type ControlType => null;

        static public Font PropertyRegularFont = new Font("Microsoft Sans Serif", 8.25f);
        static public Font PropertyModifiedFont = new Font(PropertyRegularFont, PropertyRegularFont.Style | FontStyle.Bold);

        public EdProp_Base(EdNode_Base inOwner, string inIdentifier)
        {
            OwnerNode = inOwner;
            Identifier = inIdentifier;
            if (OwnerNode.PropPanel != null && ControlType != null)
            {
                OwnerNode.PropPanel.AllPropertyControls.TryGetValue(inIdentifier, out Control);
                if (Control == null)
                {
                    throw new KeyNotFoundException($"Could not find property control with identifier: {inIdentifier}");
                }
            }
        }

        public virtual bool IsChanged()
        {
            return false;
        }

        public virtual void ApplyToDefault()
        {
            return;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free any other managed objects here
            }

            // Free any unmanaged objects here. 
            OwnerNode = null;
            Control = null;
        }

        ~EdProp_Base()
        {
            Dispose(false);
        }
    }
}
