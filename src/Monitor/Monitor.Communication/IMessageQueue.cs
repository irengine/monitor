using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monitor.Communication
{
    public interface IMessageQueue
    {
        void Enqueue(Message message);
        Message Dequeue();
    }
}
