using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using Quartz;
using Monitor.Common;
using Monitor.Communication;

namespace Monitor.Schedule
{
    class HeartbeatJob : IStatefulJob
    {
        public void Execute(JobExecutionContext context)
        {
            string projectId = context.JobDetail.JobDataMap["projectId"].ToString();
            string gatewayId = SystemInternalSetting.Projects[projectId].ToString();

            Message message = MessageFactory.CreateHeartbeatMessage(projectId, gatewayId);

            MessageQueueManager.GetSendQueue(projectId).Enqueue(message);
        }
    }
}
