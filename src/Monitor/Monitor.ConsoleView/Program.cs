using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using Monitor.Common;
using Monitor.Communication;
using Monitor.Schedule;
using System.Data;

namespace Monitor.ConsoleView
{
    class Program
    {
        private static ILog logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            logger.Info("Monitor started.");

            GetProjects();

            StartServices();

            foreach (string projectId in SystemInternalSetting.Projects.Keys)
            {
                ScheduleEngine.ScheduleHeartbeatJob(projectId);
                ScheduleEngine.ScheduleSqlJob(projectId);
            }

            // Sample report message
            MessageFactory.CreateReport("P1", "G1");

            Console.WriteLine(">>>Enter 'Exit' to exit monitor.");
            Console.Write(">");

            while (!Console.ReadLine().ToUpper().Equals("EXIT"))
            {
                Console.Write(">");
            }


            StopServices();

            logger.Info("Monitor stopped.");
        }

        public static void GetProjects()
        {
            logger.Debug("--->Show configuration<---");
            logger.Debug(SystemInternalSetting.QueryProjectsSQL);
            logger.Debug(SystemInternalSetting.QueryProjectsSQL);
            logger.Debug(SystemInternalSetting.QueryProjectUpdateSQL);

            DataSet ds = DatabaseUtility.Query(SystemInternalSetting.ConnectionString, SystemInternalSetting.QueryProjectsSQL, "");

            for (int row = 0; row < ds.Tables[0].Rows.Count; row++)
            {
                SystemInternalSetting.Projects.Add(ds.Tables[0].Rows[row]["project_id"].ToString(), ds.Tables[0].Rows[row]["gateway_id"].ToString());
            }

            logger.Info(m=>m("{0} projects included.", SystemInternalSetting.Projects.Keys.Count));
        }

        public static bool StartServices()
        {
            MessageQueueService.StartService();

            NetworkService.StartService();

            ScheduleEngine.Start();
            logger.Info("Schedule engine started.");

            return true;
        }

        public static bool StopServices()
        {
            NetworkService.StopService();

            MessageQueueService.StopService();

            ScheduleEngine.Stop();
            logger.Info("Schedule engine stopped.");

            return true;
        }
    }
}
