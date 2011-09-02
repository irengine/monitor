using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Monitor.Common;
using Common.Logging;

namespace Monitor.Communication
{
    public class MessageFactory
    {
        private static ILog logger = LogManager.GetCurrentClassLogger();

        public static Message GetMessage(byte[] data, int len)
        {
            byte[] cipherTextBytes = new byte[len - 18];
            Buffer.BlockCopy(data, 12, cipherTextBytes, 0, len - 18);
            string xml = Monitor.Common.EncryptHelper.Decrypt(cipherTextBytes);
            xml = xml.Substring(0, xml.IndexOf("</root>") + 7);

            logger.Debug("message xml:");
            logger.Debug(xml);

            Message msg = MessageParser.Deserialize(xml);

            return msg;
        }

        public static Message CreateRequestMessage(string projectId, string gatewayId)
        {
            Message msg = new Message();
            msg.common.project_id = projectId;
            msg.common.gateway_id = gatewayId;
            msg.common.type = MessageType.MESSAGE_TYPE_REQUEST;

            msg.id_validate = new ValidationSection();
            msg.id_validate.operation = MessageType.MESSAGE_TYPE_REQUEST;

            return msg;
        }

        public static byte[] CreateRequest(string projectId, string gatewayId)
        {
            string xml = MessageParser.Serialize(MessageFactory.CreateRequestMessage(projectId, gatewayId));

            return Monitor.Common.PacketHelper.Encode(xml);
        }

        public static Message GetResponse(byte[] data, int len)
        {
            return GetMessage(data, len);
        }

        public static Message CreateAuthenticationMessage(string projectId, string gatewayId, string sequence)
        {
            Message msg = new Message();
            msg.common.project_id = projectId;
            msg.common.gateway_id = gatewayId;
            msg.common.type = MessageType.MESSAGE_TYPE_AUTHENTICATION;

            msg.id_validate = new ValidationSection();
            msg.id_validate.operation = MessageType.MESSAGE_TYPE_AUTHENTICATION;
            msg.id_validate.md5 = EncryptHelper.GetSequenceMd5(sequence);

            return msg;
        }

        public static byte[] CreateAuthentication(string projectId, string gatewayId, string sequence)
        {
            string xml = MessageParser.Serialize(MessageFactory.CreateAuthenticationMessage(projectId, gatewayId, sequence));

            return Monitor.Common.PacketHelper.Encode(xml);
        }

        public static Message GetAuthenticationResult(byte[] data, int len)
        {
            return GetMessage(data, len);
        }

        public static Message CreateHeartbeatMessage(string projectId, string gatewayId)
        {
            Message msg = new Message();
            msg.common.project_id = projectId;
            msg.common.gateway_id = gatewayId;
            msg.common.type = MessageType.MESSAGE_TYPE_HEART_BEAT;

            msg.id_validate = new ValidationSection();
            msg.heart_beat = new HeartBeatSection();

            msg.id_validate.operation = MessageType.MESSAGE_TYPE_HEART_BEAT;

            return msg;
        }

        public static byte[] CreateHeartBeat(string projectId, string gatewayId)
        {
            string xml = MessageParser.Serialize(MessageFactory.CreateHeartbeatMessage(projectId, gatewayId));

            return Monitor.Common.PacketHelper.Encode(xml);
        }

        public static Message GetHeartBeatResult(byte[] data, int len)
        {
            return GetMessage(data, len);
        }

        public static Message CreateReportMessage(string projectId, string gatewayId)
        {
            Message msg = new Message();
            msg.common.project_id = projectId;
            msg.common.gateway_id = gatewayId;
            msg.common.type = MessageType.MESSAGE_TYPE_REPORT;

            if (msg.data == null)
                msg.data = new DataSection();
            msg.data.operation = MessageType.MESSAGE_TYPE_REPORT;
            msg.data.sequence = "Z1";
            msg.data.time = String.Format("{0:yyyyMMddHHmmss}", DateTime.Now);

            List<Meter> meters = new List<Meter>();
            for (int i = 1; i < 4; i++)
            {
                Meter m = new Meter();
                m.id = "X1";

                List<Function> functions = new List<Function>();
                for (int j = 0; j < 6; j++)
                {
                    Function f = new Function();
                    f.id = "Y1";
                    f.coding = "Indication" + j.ToString();
                    f.sample_time = String.Format("{0:yyyyMMddHHmmss}", DateTime.Now);
                    f.value = "s";
                    functions.Add(f);
                }
                m.function = functions.ToArray<Function>();

                meters.Add(m);
            }

            msg.data.meter = meters.ToArray<Meter>();

            return msg;
        }

        public static byte[] CreateReport(string projectId, string gatewayId)
        {
            string xml = MessageParser.Serialize(MessageFactory.CreateReportMessage(projectId, gatewayId));

            logger.Debug("report message:");
            logger.Debug(xml);

            return Monitor.Common.PacketHelper.Encode(xml);
        }
    }
}
