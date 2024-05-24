using System;
using System.Xml;
using System.Xml.Serialization;

namespace UnrealFlagEditor
{
    public abstract class XMLProperty_Base
    {
        // The identifier for this property.
        [XmlAttribute]
        public string Name;

        // If false, an error will be thrown in the console saying that the property couldn't be found.
        [XmlAttribute]
        public bool MightNotExist;

        [XmlIgnore]
        public XMLObject Owner;

        [XmlIgnore]
        public int LineNumber;

        public virtual Type ExpectedType => typeof(EdProp_Base);
        public virtual string FriendlyTypeName => "Any";

        public XMLProperty_Base()
        {
            // HACK: Get the current line number from the globally assigned reader, on construction.
            //       Maybe there's a better/simpler way to do this?
            if (EditorHeadless.CurrentReader != null && EditorHeadless.CurrentReader is IXmlLineInfo)
            {
                LineNumber = ((IXmlLineInfo)EditorHeadless.CurrentReader).LineNumber;
            }
        }

        public EdProp_Base FindProperty(EdNode_Base node)
        {
            if (node.HasInitializedProperties)
            {
                foreach (EdProp_Base prop in node.Properties)
                {
                    if (prop.Identifier != Name) continue;

                    if (ExpectedType.IsInstanceOfType(prop))
                    {
                        return prop;
                    }
                    else
                    {
                        throw new HeadlessException($"Found property {Name} within {node.GetReferencePath()}, but it isn't a {FriendlyTypeName}!");
                    }
                }
            }

            if (MightNotExist)
                return null;
            else
                throw new HeadlessException($"Could not find property {Name} within {node.GetReferencePath()}!" +
                    $" Consider adding MightNotExist=\"true\" for this tag."
                );
        }

        public abstract bool ApplyProperty(EdProp_Base prop);
        public abstract void ExtractProperty(EdProp_Base prop);

        public abstract string GetStringifiedValue();
    }

    [XmlType(TypeName = "Flag")]
    public class XMLProperty_Flag : XMLProperty_Base
    {
        [XmlAttribute]
        public bool Value;

        public override Type ExpectedType => typeof(EdProp_Checkbox);
        public override string FriendlyTypeName => "Bool";

        public override bool ApplyProperty(EdProp_Base prop)
        {
            bool OldValue = ((EdProp_Checkbox)prop).CurrentValue;
            ((EdProp_Checkbox)prop).SetValue(Value);
            return (OldValue != Value);
        }

        public override string GetStringifiedValue()
        {
            return Value.ToString();
        }

        public override void ExtractProperty(EdProp_Base prop)
        {
            Value = ((EdProp_Checkbox)prop).GetValue();
        }
    }
}
