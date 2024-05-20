using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace UnrealFlagEditor
{
    public class HeadlessInstructions
    {
        [XmlElement(ElementName = "Object")]  // Removes an extra nest.
        public List<XMLObject> Objects = new List<XMLObject>();

        [XmlAttribute]
        public bool PrintEachChange = true;

        [XmlIgnore]
        public uint ObjectsNotFound;

        [XmlIgnore]
        public uint PropertiesNotFound;

        [XmlIgnore]
        public uint AttemptedChanges;

        [XmlIgnore]
        public bool HasAnyProperties;

        public void ApplyProperties(EditorEngine edEngine)
        {
            HasAnyProperties = false;

            EdNode_Base node;
            foreach (XMLObject xmlobj in Objects)
            {
                if (xmlobj.ModifyProperties.Count > 0)
                    HasAnyProperties = true;

                xmlobj.Owner = this;
                try
                {
                    node = xmlobj.FindObject(edEngine);
                    if (node != null)
                        xmlobj.ApplyProperties(node);
                    else
                        ObjectsNotFound++;
                }
                catch (HeadlessException e)
                {
                    EditorHeadless.RegisterConsoleError(xmlobj.LineNumber, e);
                    PrintEachChange = false;
                }
            }
        }
    }

    public class HeadlessException : Exception
    {
        protected HeadlessException()
        {
        }

        public HeadlessException(string message) : base(message)
        {
        }

        public HeadlessException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
