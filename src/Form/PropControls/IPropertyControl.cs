using System.Collections.Generic;

namespace UnrealFlagEditor
{
    public interface IPropertyControl : IControlContained
    {
        // The properties this button/field is currently controlling. As of right now, this only ever has one property at a time.
        List<EdProp_Base> Prop { get; }

        string Identifier { get; }

        void UpdateStateFromProps();

        
    }

    static class IPropertyControl_ExtensionMethods
    {
        static public bool IsAnyChanged(this IPropertyControl propControl)
        {
            foreach (EdProp_Base prop in propControl.Prop)
            {
                if (prop.IsChanged())
                    return true;
            }
            return false;
        }
    }
}
