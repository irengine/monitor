using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using Quartz;
using Monitor.Common;
using System.Data;
using System.Data.SqlClient;
using Monitor.Communication;

namespace Monitor.Schedule
{
    class SqlJob : JobBase
    {
        private static ILog logger = LogManager.GetCurrentClassLogger();

        protected override void ExecuteCommand(JobExecutionContext context)
        {
            logger.Debug(m => m("Sql job:{0} has been executed.", context.JobDetail.FullName));

            JobDataMap dataMap = context.JobDetail.JobDataMap;

            projectId = dataMap["projectId"].ToString();
            gatewayId = SystemInternalSetting.Projects[projectId].ToString();
            DateTime dt = dataMap["lastTime"] == null ? DateTime.Now : dataMap.GetDateTime("lastTime");

            logger.Debug(m => m("where datetime > {0}", dt));

            string connectionString = SystemInternalSetting.ConnectionString;
            string sqlSyntax = SystemInternalSetting.QueryProjectUpdateSQL;
            sqlSyntax = string.Format(sqlSyntax, projectId, dt);

            logger.Debug(sqlSyntax);

            DateTime dt2 = DateTime.Now;

            DataSet dataSet = DatabaseUtility.Query( connectionString, sqlSyntax, "");

            if (dataSet.Tables.Count == 0)
                return;

            if (dataSet.Tables[0].Rows.Count > 0)
            {
                //Enqueue data
                Report(dataSet);
            }

            dataMap.Put("lastTime", dt2);
            ScheduleSetting.SetLastTime(projectId, dt2);
        }

        private void Report(DataSet ds)
        {
            for (int row = 0; row < ds.Tables[0].Rows.Count; row++)
            {
                Message msg = new Message();
                msg.common.project_id = projectId;
                msg.common.gateway_id = gatewayId;
                msg.common.type = MessageType.MESSAGE_TYPE_REPORT;

                if (msg.data == null)
                    msg.data = new DataSection();
                msg.data.operation = MessageType.MESSAGE_TYPE_REPORT;
                msg.data.sequence = ds.Tables[0].Rows[row]["seq"].ToString();
                msg.data.time = ds.Tables[0].Rows[row]["time"].ToString();

                List<Meter> meters = new List<Meter>();

                Meter m = new Meter();
                m.id = ds.Tables[0].Rows[row]["meter"].ToString();

                List<Function> functions = new List<Function>();

                Function f1 = new Function();
                f1.id = ds.Tables[0].Rows[row]["function_Battery_tem"].ToString();
                f1.coding = "192";
                f1.sample_time = ds.Tables[0].Rows[row]["sample_time"].ToString();
                f1.value = ds.Tables[0].Rows[row]["Battery_tem"].ToString();
                functions.Add(f1);

                Function f2 = new Function();
                f2.id = ds.Tables[0].Rows[row]["function_Irr"].ToString();
                f2.coding = "192";
                f2.sample_time = ds.Tables[0].Rows[row]["sample_time"].ToString();
                f2.value = ds.Tables[0].Rows[row]["Irr"].ToString();
                functions.Add(f2);

                Function f3 = new Function();
                f3.id = ds.Tables[0].Rows[row]["function_Sur_tem"].ToString();
                f3.coding = "192";
                f3.sample_time = ds.Tables[0].Rows[row]["sample_time"].ToString();
                f3.value = ds.Tables[0].Rows[row]["Sur_tem"].ToString();
                functions.Add(f3);

                Function f4 = new Function();
                f4.id = ds.Tables[0].Rows[row]["function_Totalampac"].ToString();
                f4.coding = "192";
                f4.sample_time = ds.Tables[0].Rows[row]["sample_time"].ToString();
                f4.value = ds.Tables[0].Rows[row]["Totalampac"].ToString();
                functions.Add(f4);

                m.function = functions.ToArray<Function>();

                meters.Add(m);

                msg.data.meter = meters.ToArray<Meter>();

                string xml = MessageParser.Serialize(msg);
                logger.Debug(xml);

                MessageQueueManager.GetSendQueue(projectId).Enqueue(msg);


 
            }
        }
    }
}
