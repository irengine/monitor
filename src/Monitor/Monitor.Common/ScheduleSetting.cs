using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;

namespace Monitor.Common
{
    public class ScheduleSetting
    {
        private static Hashtable settings = new Hashtable();
        private static Object syncLock = new Object();

        private static ILog log = LogManager.GetCurrentClassLogger();

        //public static string GetFrequency(string code)
        //{
        //    string frequency = "30 0/5 * * * ?";
        //    lock (syncLock)
        //    {
        //        if (!settings.ContainsKey(code))
        //        {
        //            ScheduleConfiguration conf = new ScheduleConfiguration(code);
        //            conf.FREQUENCY = frequency;
        //            settings[code] = conf;
        //        }

        //        frequency = ((ScheduleConfiguration)settings[code]).FREQUENCY;
        //    }
        //    return frequency;
        //}

        public static DateTime GetLastTime(String code)
        {
            code = "P" + code;
            DateTime lastTime = new DateTime(2000,1,1);
            lock (syncLock)
            {
                if (!settings.ContainsKey(code))
                {
                    log.Debug(m=>m("Do not find last time for {0}, set with default date", code));
                    ScheduleConfiguration conf = new ScheduleConfiguration(code);

                    if (conf.LastTime < lastTime)
                        conf.LastTime = lastTime;

                    settings[code] = conf;
                }

                log.Debug(m => m("current last time for code is {0}", ((ScheduleConfiguration)settings[code]).LastTime));
                lastTime = ((ScheduleConfiguration)settings[code]).LastTime;
            }
            return lastTime;
        }

        public static void SetLastTime(String code, DateTime lastTime)
        {
            code = "P" + code;
            lock (syncLock)
            {
                if (!settings.ContainsKey(code))
                {
                    ScheduleConfiguration conf = new ScheduleConfiguration(code);
                    settings[code] = conf;
                }

                ((ScheduleConfiguration)settings[code]).LastTime = lastTime;
                ((ScheduleConfiguration)settings[code]).WriteKeysToConfig();
            }
        }
    }
}
