using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Monitor.Communication
{
    [XmlRoot(ElementName = "root")]
    public class Message
    {
        public Message()
        {
            common = new GeneralSection();
            //id_validate = new ValidationSection();
            //heart_beat = new HeartBeatSection();
            //data = new DataSection();
        }

        public GeneralSection common;
        public ValidationSection id_validate;
        public HeartBeatSection heart_beat;
        public DataSection data;

        [XmlIgnore]
        public bool id_validateSpecified { get { return id_validate != null; } }
        [XmlIgnore]
        public bool heart_beatSpecified { get { return heart_beat != null; } }
        [XmlIgnore]
        public bool dataSpecified { get { return data != null; } }
    }

    public class GeneralSection
    {
        public string project_id;
        public string gateway_id;
        public string type;
    }

    public class ValidationSection
    {
        [XmlAttribute]
        public string operation;
        public string sequence;
        public string md5;
        public string result;
    }

    public class HeartBeatSection
    {
        [XmlAttribute]
        public string operation;
    }

    public class DataSection
    {
        [XmlAttribute]
        public string operation;
        public string sequence;
        public string parse = "yes";
        public string time;
        [XmlElement]
        public Meter[] meter;
    }

    public class Meter
    {
        [XmlAttribute]
        public string id;
        [XmlAttribute]
        public string conn = "conn";
        [XmlElement]
        public Function[] function;
    }

    public class Function
    {
        [XmlAttribute]
        public string id;
        [XmlAttribute]
        public string coding;
        [XmlAttribute]
        public int error = 0;
        [XmlAttribute]
        public string sample_time;
        [XmlText]
        public string value;
    }
}
