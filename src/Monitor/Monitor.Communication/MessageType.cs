using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monitor.Communication
{
    public class MessageType
    {
        public static string MESSAGE_TYPE_REQUEST = "request";
        public static string MESSAGE_TYPE_SEQUENCE = "sequence";
        public static string MESSAGE_TYPE_AUTHENTICATION = "md5";
        public static string MESSAGE_TYPE_AUTHENTICATION_ACK = "result";
        public static string MESSAGE_TYPE_HEART_BEAT = "notify";
        public static string MESSAGE_TYPE_HEART_BEAT_ACK = "heart_result";
        public static string MESSAGE_TYPE_REPORT = "report";
    }
}
