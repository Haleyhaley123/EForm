using Common;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManager.ScheduleJobs
{
    public class JobScheduler
    {
        public static void Start()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            #region Đồng bộ dịch vụ OH
            IJobDetail sync_oh_service_job = JobBuilder.Create<SyncOHServiceJob>().Build();
            ITrigger sync_oh_service_trigger = TriggerBuilder.Create()
                .WithCronSchedule(ConfigHelper.CF_SyncOHService_CS)
                .Build();
            scheduler.ScheduleJob(sync_oh_service_job, sync_oh_service_trigger);
            CustomLog.Instant.IntervalJobLog("SyncOHService job was created with Interval " + ConfigHelper.CF_SyncOHService_CS, Constant.Log_Type_Info, printConsole: true);
            #endregion .Đồng bộ dịch vụ OH

            #region gửi mail thông báo ngày trả kết quả
            IJobDetail send_mail_notifications_job = JobBuilder.Create<SendMailNotificationsJob>().Build();
            ITrigger send_mail_notifications_job_trigger = TriggerBuilder.Create()
                .WithCronSchedule(ConfigHelper.CF_SendMailNotifications_CS)
                .Build();
            scheduler.ScheduleJob(send_mail_notifications_job, send_mail_notifications_job_trigger);
            CustomLog.Instant.IntervalJobLog("SendMailNotifications job was created with Interval " + ConfigHelper.CF_SendMailNotifications_CS, Constant.Log_Type_Info, printConsole: true);
            #endregion gửi mail thông báo ngày trả kết quả

            #region Xóa tmp log And Noti
            IJobDetail clearOldTmpLogAndNoti = JobBuilder.Create<ClearOldTmpLogAndNoti>().Build();
            ITrigger clearOldTmpLogAndNotiJobTrigger = TriggerBuilder.Create()
                .WithCronSchedule(ConfigHelper.CF_ClearOldNotifications_CS)
                .Build();
            scheduler.ScheduleJob(clearOldTmpLogAndNoti, clearOldTmpLogAndNotiJobTrigger);
            CustomLog.Instant.IntervalJobLog("Clear old log tmp And Noti job was created with Interval " + ConfigHelper.CF_ClearOldNotifications_CS, Constant.Log_Type_Info, printConsole: true);
            #endregion Xóa tmp log And Noti

            #region Xóa null data
            IJobDetail clearNullData = JobBuilder.Create<ClearNullData>().Build();
            ITrigger clearNullDataJobTrigger = TriggerBuilder.Create()
                .WithCronSchedule(ConfigHelper.CF_NullData_CS)
                .Build();
            scheduler.ScheduleJob(clearNullData, clearNullDataJobTrigger);
            CustomLog.Instant.IntervalJobLog("Clear NULL DATA job was created with Interval " + ConfigHelper.CF_NullData_CS, Constant.Log_Type_Info, printConsole: true);
            #endregion Xóa null data

            #region move Logs Data
            IJobDetail moveLogsData = JobBuilder.Create<MoveLogsData>().Build();
            ITrigger moveLogsDataJobTrigger = TriggerBuilder.Create()
                .WithCronSchedule(ConfigHelper.CF_MoveLogData_CS)
                .Build();
            scheduler.ScheduleJob(moveLogsData, moveLogsDataJobTrigger);
            CustomLog.Instant.IntervalJobLog("Move log job was created with Interval " + ConfigHelper.CF_MoveLogData_CS, Constant.Log_Type_Info, printConsole: true);
            #endregion move Logs Data


            #region send noti to myvinmec
            IJobDetail sendnotitomyvinmec = JobBuilder.Create<SendSMSUtilDischargedJob>().Build();
            ITrigger sendnotitomyvinmecTrigger = TriggerBuilder.Create()
                .WithCronSchedule(ConfigHelper.CF_SendNotiToMyVinmec_CS)
                .Build();
            scheduler.ScheduleJob(sendnotitomyvinmec, sendnotitomyvinmecTrigger);
            CustomLog.Instant.IntervalJobLog("SendNotiToMyVinmec was created with Interval " + ConfigHelper.CF_SendNotiToMyVinmec_CS, Constant.Log_Type_Info, printConsole: true);
            #endregion send noti to myvinmec

            // Lock VIP patient 
            #region Lock VIP patient 
            IJobDetail LockVipMedicalRecordJob = JobBuilder.Create<LockVipMedicalRecord>().Build();
            ITrigger LockVipMedicalRecordJobTrigger = TriggerBuilder.Create()
                .WithCronSchedule(ConfigHelper.CF_LockVipPatientService_CS)
                .Build();
            scheduler.ScheduleJob(LockVipMedicalRecordJob, LockVipMedicalRecordJobTrigger);
            CustomLog.Instant.IntervalJobLog("LockVipMedicalRecordJob was created with Interval" + ConfigHelper.CF_LockVipPatientService_CS, Constant.Log_Type_Info, printConsole: true);
            #endregion Lock VIP patient

            #region send noti api getway 
            IJobDetail notifyAPIGW = JobBuilder.Create<NotifyAPIGWService>().Build();
            ITrigger notifyAPIGWTrigger = TriggerBuilder.Create()
                .WithCronSchedule(ConfigHelper.CF_NotifyAPIGW)
                .Build();
            scheduler.ScheduleJob(notifyAPIGW, notifyAPIGWTrigger);
            CustomLog.Instant.IntervalJobLog("SendNotiToMyVinmec was created with Interval " + ConfigHelper.CF_NotifyAPIGW, Constant.Log_Type_Info, printConsole: true);
            #endregion send noti api getway
            #region Đồng bộ Charge
            IJobDetail sync_data_job = JobBuilder.Create<SyncDataHC>().Build();
            ITrigger sync_datahc_trigger = TriggerBuilder.Create()
                .WithCronSchedule(ConfigHelper.CF_SyncOHHCService_C)
                .Build();
            scheduler.ScheduleJob(sync_data_job, sync_datahc_trigger);
            CustomLog.Instant.IntervalJobLog("SyncDataHC job was created with Interval " + ConfigHelper.CF_SyncOHHCService_C, Constant.Log_Type_Info, printConsole: true);

            IJobDetail sync_data_PathologyMicrobiology_job = JobBuilder.Create<SyncDataPathologyMicrobiologyHC>().Build();
            ITrigger sync_datahc_PathologyMicrobiology_trigger = TriggerBuilder.Create()
                .WithCronSchedule(ConfigHelper.CF_SyncOHHCPathologyMicrobiologyService_C)
                .Build();
            scheduler.ScheduleJob(sync_data_PathologyMicrobiology_job, sync_datahc_PathologyMicrobiology_trigger);
            CustomLog.Instant.IntervalJobLog("SyncDataPathologyMicrobiologyHC job was created with Interval " + ConfigHelper.CF_SyncOHHCPathologyMicrobiologyService_C, Constant.Log_Type_Info, printConsole: true);
            #endregion
        }
    }
}