using System;
using Common;
using Quartz;
using DataAccess.Dapper.Repository;
using System.Configuration;

namespace SyncManager.ScheduleJobs
{
    public class MoveLogsData : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            DoJob();
        }

        public void Execute()
        {
            DoJob();
        }

        private void DoJob()
        {
            CustomLog.intervaljoblog.Info($"<MoveLogsData> Start!");
            try
            {
                var count = ConfigurationManager.AppSettings["MaximumNumberOfItemPerRequest"] != null ? int.Parse(ConfigurationManager.AppSettings["MaximumNumberOfItemPerRequest"].ToString()) : 1000;
                var h = DateTime.Now.Hour;
                if (h > 7 & h < 19)
                {
                    count = 1000;
                } else
                {
                    // count = count * 2;
                }
                var param = new
                {
                    takeRowNumber = count
                };
                ExecStoProcedure.NoResult("spMoveDataTableLogToDBOther", param);
                CustomLog.intervaljoblog.Info($"<MoveLogsData> Success!");
            }
            catch (Exception ex)
            {
                CustomLog.intervaljoblog.Info(string.Format("<MoveLogsData> Error: {0}", ex));
            }
        }
    }
}