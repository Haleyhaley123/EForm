using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class ConfigHelper 
    {
        public static string AppName { get { return ConfigurationManager.AppSettings["AppName"] != null ? ConfigurationManager.AppSettings["AppName"].ToString() : string.Empty; } }
        public static string CF_SyncOHHCService_C { get { return ConfigurationManager.AppSettings["CF_SyncOHHCService_C"] != null ? ConfigurationManager.AppSettings["CF_SyncOHHCService_C"].ToString() : "0 30 0/1 ? * * *"; } }
        public static string CF_SyncOHHCPathologyMicrobiologyService_C { get { return ConfigurationManager.AppSettings["CF_SyncOHHCPathologyMicrobiologyService_C"] != null ? ConfigurationManager.AppSettings["CF_SyncOHHCPathologyMicrobiologyService_C"].ToString() : "0 30 0/1 ? * * *"; } }
        public static string CF_SyncOHService_CS { get { return ConfigurationManager.AppSettings["SyncOHService_CS"] != null ? ConfigurationManager.AppSettings["SyncOHService_CS"].ToString() : "0 0/5 0/1 ? * * *"; } }
        //public static string CF_SyncCpoeOrderable_CS { get { return ConfigurationManager.AppSettings["SyncCpoeOrderable_CS"] != null ? ConfigurationManager.AppSettings["SyncCpoeOrderable_CS"].ToString() : "0 0/5 0/1 ? * * *"; } }
        //public static string CF_SyncRadiololyProcedure_CS { get { return ConfigurationManager.AppSettings["SyncRadiololyProcedure_CS"] != null ? ConfigurationManager.AppSettings["SyncRadiololyProcedure_CS"].ToString() : "0 0/5 0/1 ? * * *"; } }
        public static string CF_ClearOldNotifications_CS { get { return ConfigurationManager.AppSettings["CF_ClearOldNotifications_CS"] != null ? ConfigurationManager.AppSettings["CF_ClearOldNotifications_CS"].ToString() : "0 0/15 0-6 * * ?"; } }
        public static string CF_MoveLogData_CS { get { return ConfigurationManager.AppSettings["CF_MoveLogData_CS"] != null ? ConfigurationManager.AppSettings["CF_MoveLogData_CS"].ToString() : "0 0/5 0-6,18-23 * * ?"; } }
        public static string CF_LockVipPatientService_CS { get { return ConfigurationManager.AppSettings["CF_LockVipPatientService_CS"] != null ? ConfigurationManager.AppSettings["CF_LockVipPatientService_CS"].ToString() : "0 0/45 0/1 ? * * *"; } }
        public static string CF_SendMailNotifications_CS { get { return ConfigurationManager.AppSettings["CF_SendMailNotifications_CS"] != null ? ConfigurationManager.AppSettings["CF_SendMailNotifications_CS"].ToString() : "0 0/5 0/1 ? * * *"; } }
        public static string CF_SendNotiToMyVinmec_CS { get { return ConfigurationManager.AppSettings["CF_SendNotiToMyVinmec_CS"] != null ? ConfigurationManager.AppSettings["CF_SendNotiToMyVinmec_CS"].ToString() : "0 0/5 0/1 ? * * *"; } }
        public static string CF_NullData_CS { get { return ConfigurationManager.AppSettings["CF_NullData_CS"] != null ? ConfigurationManager.AppSettings["CF_NullData_CS"].ToString() : "0 0/5 0-6,18-23 * * ?"; } }
        public static string CF_NotifyAPIGW { get { return ConfigurationManager.AppSettings["CF_NotifyAPIGW"] != null ? ConfigurationManager.AppSettings["CF_NotifyAPIGW"].ToString() : "0 1 0 ? * * *"; } }
        //
    }
}
