using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using Monitor.Common;

namespace Monitor.Communication
{
    public class MessageParser
    {
        public static string Serialize(Message msg)
        {
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(Message));

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

            //  Add lib namespace with empty prefix
            ns.Add("", "");

            Encoding ec = SystemInternalSetting.Encoding;

            string xml = Serialize(x, ec, ns, false, msg);

            //ISSUE: remove first character
            return xml.Substring(1);
        }

        private static string Serialize(XmlSerializer serializer,
                       Encoding encoding,
                       XmlSerializerNamespaces ns,
                       bool omitDeclaration,
                       object objectToSerialize)
        {
            MemoryStream ms = new MemoryStream();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = false;
            settings.OmitXmlDeclaration = omitDeclaration;
            settings.Encoding = encoding;
            XmlWriter writer = XmlWriter.Create(ms, settings);
            serializer.Serialize(writer, objectToSerialize, ns);
            return encoding.GetString(ms.ToArray()); ;
        }

        public static Message Deserialize(string xml)
        {
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(Message));
            return x.Deserialize(new StringReader(xml)) as Message;
        }
    }
}
