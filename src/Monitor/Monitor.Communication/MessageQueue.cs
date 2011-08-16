using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;

namespace Monitor.Communication
{
    public class MessageQueue : IMessageQueue
    {
        private static ILog logger = LogManager.GetCurrentClassLogger();
        private string queueName;

        public string QueueName
        {
            get { return queueName; }
            set { queueName = value; }
        }

        private Queue<Message> queue;

        private object queueLock = new object();

        public MessageQueue(string name)
        {
            this.queueName = name;
            lock (queueLock)
            {
                this.queue = new Queue<Message>();
            }
        }

        public void Enqueue(Message message)
        {
            lock (queueLock)
            {
                //logger.Debug(m => m("Queue {0} have {1} messages.", queueName, queue.Count));
                queue.Enqueue(message);
            }
        }

        public Message Dequeue()
        {
            lock (queueLock)
            {
                //logger.Debug(m => m("Queue {0} have {1} messages.", queueName, queue.Count));
                if (queue.Count > 0)
                    return queue.Dequeue();
                else
                    return null;
            }
        }
    }
}
