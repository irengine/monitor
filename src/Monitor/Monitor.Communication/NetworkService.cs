using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Monitor.Common;
using System.Collections;

namespace Monitor.Communication
{
    public class NetworkService
    {
        private static Hashtable handlers = new Hashtable();

        public static void StartService()
        {
            foreach (string project_id in SystemInternalSetting.Projects.Keys)
            {
                SocketHandler handler = new SocketHandler(SystemInternalSetting.Ip, SystemInternalSetting.Port);
                handlers.Add(project_id, handler);
            }

            foreach (DictionaryEntry de in handlers)
            {
                SocketHandler handler = de.Value as SocketHandler;
                handler.Start();

                string projectId = de.Key.ToString();
                string gatewayId = SystemInternalSetting.Projects[projectId].ToString();
                handler.Handshake(projectId, gatewayId);
            }
        }

        public static void StopService()
        {
            foreach (SocketHandler handler in handlers.Values)
            {
                handler.Stop();
            }

        }

        public static SocketHandler GetHandler(string name)
        {
            return handlers[name] as SocketHandler;
        }
    }
}
