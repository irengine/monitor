using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Monitor.Common;

namespace Monitor.Communication
{
    public class MessageQueueService
    {
        private static List<MessageQueueDaemon> daemons = new List<MessageQueueDaemon>();

        public static void StartService()
        {
            foreach (string project_id in SystemInternalSetting.Projects.Keys)
            {
                MessageQueueDaemon daemon = new MessageQueueDaemon(project_id);
                daemons.Add(daemon);
            }

            foreach (MessageQueueDaemon daemon in daemons)
            {
                daemon.Initialize();
                daemon.Start();
            }
        }

        public static void StopService()
        {
            foreach (MessageQueueDaemon daemon in daemons)
            {
                daemon.Stop();
                daemon.Shutdown();
            }

        }
    }
}
