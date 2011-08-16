using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using Quartz.Impl;

namespace Monitor.Schedule
{
    public class ScheduleEngine
    {
        private static IScheduler engine = null;

        private static void Initialize()
        {
            if (engine == null)
            {
                ISchedulerFactory sf = new StdSchedulerFactory();
                engine = sf.GetScheduler();
            }
        }

        public static void Start()
        {
            Initialize();

            engine.Start();
        }

        public static void Stop()
        {
            engine.Shutdown(true);
        }

        public static void Schedule()
        {

        }

        private static string ONE_MINUTE_PATTERN = "0 0/1 * * * ?";
        private static string FIVE_MINUTE_PATTERN = "0 0/5 * * * ?";

        public static void ScheduleHeartbeatJob(string projectId)
        {
            string jobName = "Heartbeat_" + projectId;
            JobDetail job = new JobDetail(jobName, jobName, typeof(HeartbeatJob));
            job.JobDataMap["projectId"] = projectId;

            CronTrigger trigger = new CronTrigger(jobName, jobName, jobName, jobName, ONE_MINUTE_PATTERN);
            engine.AddJob(job, true);
            DateTime ft = engine.ScheduleJob(trigger);
        }

        public static void ScheduleSqlJob(string projectId)
        {
            string jobName = "Report_" + projectId;
            JobDetail job = new JobDetail(jobName, jobName, typeof(SqlJob));
            job.JobDataMap["projectId"] = projectId;
            job.JobDataMap["lastTime"] = Monitor.Common.ScheduleSetting.GetLastTime(projectId);

            CronTrigger trigger = new CronTrigger(jobName, jobName, jobName, jobName, FIVE_MINUTE_PATTERN);
            engine.AddJob(job, true);
            DateTime ft = engine.ScheduleJob(trigger);
        }
    }
}
