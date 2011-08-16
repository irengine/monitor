using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Monitor.Common;
using Common.Logging;

namespace Monitor.Communication
{
    public class SocketHandlerWard : Ward
    {
        private static ILog logger = LogManager.GetCurrentClassLogger();
        private SocketHandler handler = null;

        public SocketHandlerWard(SocketHandler handler)
            : base()
        {
            this.handler = handler;
        }

        protected override void Execute()
        {
            handler.Stop();
            handler.Start();
            handler.Handshake();
        }
    }
}
