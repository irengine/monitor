using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using Common.Logging;

namespace Monitor.Schedule
{
    class JobBase : IStatefulJob
    {
        private static ILog logger = LogManager.GetCurrentClassLogger();

        protected virtual void ExecuteCommand(JobExecutionContext context) { }

        protected string projectId = "";
        protected string gatewayId = "";


        public void Execute(JobExecutionContext context)
        {
            try
            {
                ExecuteCommand(context);
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }
    }
}
