using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Monitor.Common;
using Common.Logging;

namespace Monitor.Communication
{
    public class MessageQueueDaemon : Daemon
    {
        private static ILog logger = LogManager.GetCurrentClassLogger();
        //private IMessageQueue queue = null;
        //private SocketHandler handler = null;

        public MessageQueueDaemon(string daemonName) : base(daemonName)
        {
            //queue = MessageQueueManager.GetSendQueue(Name);
            //handler = NetworkService.GetHandler(Name);
        }
        protected override void Execute()
        {
            Message message = MessageQueueManager.GetSendQueue(Name).Dequeue();
            if (null == message)
            {
                System.Threading.Thread.Sleep(5000);
            }
            else
            {
                Send(message);
                Receive();
            }
        }

        private void Send(Message message)
        {
            string xml = MessageParser.Serialize(message);

            byte[] bytes = Monitor.Common.PacketHelper.Encode(xml);

            int bytesSend = NetworkService.GetHandler(Name).Send(bytes);

            logger.Debug(m => m("Send {0} bytes", bytesSend));
        }

        private void Receive()
        {
            byte[] bytes = new byte[1024];

            int bytesRec = NetworkService.GetHandler(Name).Receive(bytes);

            logger.Debug(m => m("Receive {0} bytes", bytesRec));
        }
    }
}
