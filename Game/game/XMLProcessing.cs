using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public abstract class XMLProcessing
	{
        protected XmlElement xmlElement;
        abstract public void LoadXml(XmlElement xmlElement);
        abstract public XmlElement SaveXml(XmlDocument doc);

        protected XmlDocument loadXmlDocument(string filename)
        {
            TextReader tr = new StreamReader(filename, System.Text.Encoding.GetEncoding(1251));
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(tr.ReadToEnd());
            tr.Close();
            return doc;
        }

        public bool XMLAssignBoolean(string source, bool defaultValue)
        {
            if (string.IsNullOrEmpty(source))
                return defaultValue;
            return Convert.ToBoolean(source);
        }

        public int XMLAssignInteger(string source, int defaultValue)
        {
            try
            {
                if (string.IsNullOrEmpty(source))
                    return defaultValue;
                return Convert.ToInt32(source);
            }
            catch (FormatException ex)
            {
                throw new Exception("Trying to assign integer from the value: " + source, ex);
            }
        }

        protected double XMLAssignDouble(string source, double defaultValue)
        {
            if (string.IsNullOrEmpty(source))
                return defaultValue;
            return Convert.ToDouble(source);
        }

        protected string XMLAssignString(string source, string defaultValue)
        {
            if (string.IsNullOrEmpty(source))
                return defaultValue;
            return source;
        }

    }	
}
