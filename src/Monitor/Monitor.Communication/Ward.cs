using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using Monitor.Common;

namespace Monitor.Communication
{
    public abstract class Ward
    {
        private static ILog logger = LogManager.GetCurrentClassLogger();
        private IActiveObject signal = null;

        public Ward()
        {
            signal = new SignalActiveObject();
        }

        public void Initialize()
        {
            signal.Initialize("Ward", Execute);
        }

        public void Signal()
        {
            signal.Signal();
        }

        public void Shutdown()
        {
            signal.Shutdown();
        }

        protected virtual void Execute() {}
    }
}
