using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Constant
    {
        #region Log
        public readonly static string Log_Type_Info = "Log_Info";
        public readonly static string Log_Type_Debug = "Log_Debug";
        public readonly static string Log_Type_Error = "Log_Error";
        #endregion
        #region Datetime format
        public readonly static string DATE_SQL = "yyyy-MM-dd";
        public readonly static string DATETIME_SQL = "yyyy-MM-dd HH:mm:ss";
        public readonly static string TIME_FORMAT = "HH:mm:ss";
        public readonly static string TIME_FORMAT_WITHOUT_SECOND = "HH:mm";
        public readonly static string TIME_DATE_FORMAT = "HH:mm:ss dd/MM/yyyy";
        public readonly static string TIME_DATE_FORMAT_WITHOUT_SECOND = "HH:mm dd/MM/yyyy";
        public readonly static string MONTH_YEAR_FORMAT = "MM/yyyy";
        public readonly static string YEAR_MONTH_FORMAT = "yyyyMM";
        public readonly static string DATE_FORMAT = "dd/MM/yyyy";
        public readonly static string DATE_TIME_FORMAT = "dd/MM/yyyy HH:mm:ss";
        public readonly static string DATE_TIME_FORMAT_WITHOUT_SECOND = "dd/MM/yyyy HH:mm";
        #endregion
        #region DataFormatType
        public readonly static string FM_TABLE = "TABLE";
        public readonly static string FM_EXP_EXCEL = "EXP_EXCEL";
        #endregion .DataFormatType
        #region HIS
        public readonly static Dictionary<string, int> HIS_CODE = new Dictionary<string, int> {
            { "OH", 0 },
            { "EHos", 1 }
        };
        #endregion
        public class ChargeItemType
        {
            public static string Lab = "Lab";
            public static string Rad = "Rad";
            public static string Allies = "Allies";
        };
        public readonly static string VMHC_CODE = "VIHC";
    }
}
