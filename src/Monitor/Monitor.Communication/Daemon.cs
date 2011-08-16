using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using Monitor.Common;

namespace Monitor.Communication
{
    public abstract class Daemon
    {
        private static ILog logger = LogManager.GetCurrentClassLogger();
        private IActiveObject daemon = null;
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public Daemon(string daemonName)
        {
            daemon = new DaemonActiveObject();
            name = daemonName;
        }

        public void Initialize()
        {
            daemon.Initialize(name, Execute);
        }

        public void Start()
        {
            daemon.Start();
        }

        public void Stop()
        {
            daemon.Stop();
        }

        public void Shutdown()
        {
            daemon.Shutdown();
        }

        protected virtual void Execute() {}
    }
}
