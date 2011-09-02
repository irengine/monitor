using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using Common.Logging;
using Monitor.Common;
using Monitor.Schedule;
using Monitor.Communication;
using System.Data;

namespace Monitor.Tray
{
    static class Program
    {
        private static ILog logger = LogManager.GetCurrentClassLogger();
        public static NotifyIcon appIcon;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            System.Threading.Mutex appSingleTon = new Mutex(false, "Service Monitor");
            if (appSingleTon.WaitOne(0, false))
            {
                Application.EnableVisualStyles();
                Program.IntializeIcon();
                Microsoft.Win32.SystemEvents.SessionEnded += new
                        Microsoft.Win32.SessionEndedEventHandler(SystemEvent_SessionEnded);



                // Start before Application Run
                appIcon.ContextMenu.MenuItems[0].Enabled = false;
                appIcon.ContextMenu.MenuItems[1].Enabled = true;
                appIcon.Icon = Monitor.Tray.Properties.Resources.ServiceStart;

                Start();

                Application.Run();
            }
            appSingleTon.Close();
        }

        private static void IntializeIcon()
        {
            appIcon = new NotifyIcon();
            appIcon.Icon = Monitor.Tray.Properties.Resources.ServiceStop;
            appIcon.Visible = true;

            ContextMenu mnu = new ContextMenu();

            MenuItem startItem = new MenuItem("Start");
            //startItem.DefaultItem = true;
            startItem.Click += new EventHandler(StartItem_Click);
            mnu.MenuItems.Add(startItem);

            MenuItem stopItem = new MenuItem("Stop");
            //stopItem.DefaultItem = true;
            stopItem.Enabled = false;
            stopItem.Click += new EventHandler(StopItem_Click);
            mnu.MenuItems.Add(stopItem);


            mnu.MenuItems.Add("-");
            MenuItem exitItem = new MenuItem("Exit");
            exitItem.Click += new EventHandler(ExitItem_Click);
            mnu.MenuItems.Add(exitItem);


            appIcon.ContextMenu = mnu;
            appIcon.Text = "Monitor Manager";
        }

        private static void SystemEvent_SessionEnded(object sender, Microsoft.Win32.SessionEndedEventArgs e)
        {
            Stop();

            appIcon.Visible = false;
            Application.Exit();
        }

        private static void StartItem_Click(object sender, EventArgs e)
        {
            appIcon.ContextMenu.MenuItems[0].Enabled = false;
            appIcon.ContextMenu.MenuItems[1].Enabled = true;
            appIcon.Icon = Monitor.Tray.Properties.Resources.ServiceStart;

            Start();
        }

        private static void StopItem_Click(object sender, EventArgs e)
        {
            appIcon.ContextMenu.MenuItems[0].Enabled = true;
            appIcon.ContextMenu.MenuItems[1].Enabled = false;
            appIcon.Icon = Monitor.Tray.Properties.Resources.ServiceStop;

            Stop();
        }

        private static void ExitItem_Click(object sender, EventArgs e)
        {
            Stop();

            appIcon.Visible = false;
            Application.Exit();
        }

        private static void Start()
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
        }

        private static void Stop()
        {
            StopServices();

            logger.Info("Monitor stopped.");
        }

        public static void GetProjects()
        {
            logger.Debug("--->Show configuration<---");
            logger.Debug(SystemInternalSetting.QueryProjectsSQL);
            logger.Debug(SystemInternalSetting.QueryProjectUpdateSQL);

            DataSet ds = DatabaseUtility.Query(SystemInternalSetting.ConnectionString, SystemInternalSetting.QueryProjectsSQL, "");

            for (int row = 0; row < ds.Tables[0].Rows.Count; row++)
            {
                SystemInternalSetting.Projects.Add(ds.Tables[0].Rows[row]["project_id"].ToString(), ds.Tables[0].Rows[row]["gateway_id"].ToString());
            }

            logger.Info(m => m("{0} projects included.", SystemInternalSetting.Projects.Keys.Count));
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
