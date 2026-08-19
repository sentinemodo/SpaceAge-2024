using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public abstract class NamedObject  : XMLProcessing, ISingular
	{
        public const int MaxNameLength = 6;

		protected string name = string.Empty;
		public string Name
		{
			get { return this.name; }
			set 
			{
                //if (value.Length > NameObject.MaxNameLength)
                //    throw new ArgumentOutOfRangeException("too long name. received: " + value);
				this.name = value; 
			}
		}

        public virtual string Alias { get; set; }

        public string FullName { get; set; }


		public virtual string ReportName
		{
			get { return this.FullName + " [" + this.Name + "]"; }
		}

		public string Description { get; set; }

        public string GenerateRandomIdentifier(string prefix, int length = NamedObject.MaxNameLength)
		{
			return string.Concat(prefix + Sequence.GenerateRandomString(length, "Random identifier"));
		}

		public string GenerateRandomIdentifier()
		{
            return this.GenerateRandomIdentifier(string.Empty);
		}

		public string GenerateRandomIdentifier(string prefix)
		{
            return this.GenerateRandomIdentifier(prefix, NamedObject.MaxNameLength - prefix.Length);
		}

		public NamedObject(string name)
		{
			this.Name = name;
		}

		public override string ToString()
		{
			return this.ReportName;
		}

        public virtual Location Location
        {
            get { return null; }
        }


        public override void LoadXml(XmlElement xmlObject)
        {
            if (xmlObject.HasAttribute("name-en"))
            {
                this.FullName = xmlObject.GetAttribute("name-en");
            }
            if (xmlObject.HasAttribute("description"))
            {
                this.Description = xmlObject.GetAttribute("description");
            }
        }

        public void LoadMultipleNames(XmlElement element)
        {
            IMultiple namedObject = this as IMultiple;
            if (namedObject == null)
            {
                return;
            }
            if (element.HasAttribute("name-en2"))
            {
                namedObject.FullNameMultiple = element.GetAttribute("name-en2");
            }
            else
            {
                namedObject.FullNameMultiple = namedObject.FullName;
            }
        }

        public XmlElement SaveXml(XmlDocument doc, string objectType)
        {
            this.xmlElement = doc.CreateElement(objectType);
            this.xmlElement.SetAttribute("name", this.Name);
            if (this.FullName != null & this.FullName != string.Empty)
            {
                this.xmlElement.SetAttribute("name-en", this.FullName);
            } 
            return this.xmlElement;
        }

        public override XmlElement SaveXml(XmlDocument doc)
        {
            if (this.xmlElement == null) 
            {
                this.xmlElement = doc.CreateElement("named-object");
            }
            this.xmlElement.SetAttribute("name", this.Name);
            if (this.FullName != string.Empty)
            {
                this.xmlElement.SetAttribute("name-en", this.FullName);
            }
            return this.xmlElement;
        }
    }
}
