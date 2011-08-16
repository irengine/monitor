using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Monitor.Communication
{
    public class MessageQueueManager
    {
        private static Hashtable queues = new Hashtable();
        private static object queuesLock = new object();

        public static IMessageQueue GetSendQueue(string name)
        {
            return GetQueue("SEND_QUEUE_", name);
        }

        public static IMessageQueue GetReceiveQueue(string name)
        {
            return GetQueue("RECEIVE_QUEUE_", name);
        }

        private static IMessageQueue GetQueue(string prefix, string name)
        {
            IMessageQueue queue = null;

            lock (queuesLock)
            {
                string queueName = prefix + name;
                if (null == queues[queueName])
                {
                    queue = new MessageQueue(queueName);
                    queues[queueName] = queue;
                }
                else
                {
                    queue = queues[queueName] as IMessageQueue;
                }
            }

            return queue;
        }
    }
}
