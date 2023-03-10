using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EForm.Models
{
    public class ICDModel
    {
        public string Code { get; set; }
        public string Label { get; set; }
    }
    public class ICDJsonModel
    {
        public string code { get; set; }
        public string label { get; set; }
        public string ViName { get; set; }
        public string EnName { get; set; }
    }
    public class DiagnosisAndICDModel
    {
        public string ICD { get; set; }
        public string ICDString { get; set; }
        public string Diagnosis { get; set; }
        public string ICDOption { get; set; }
        public string ICDOptionString { get; set; }
        public string DiagnosisOption { get; set; }
        public string Reason { get; set; }
        public string VisitType { get; set; }
    }
    public class TransferVisitInfoModel
    {
        public string ICD { get; set; }
        public string Diagnosis { get; set; }
        public string ResonForTransfer { get; set; }
        public string SpecialtyName { get; set; }
    }
    public class UserInfo
    {
        public string userId { get; set; }
        public string userName { get; set; }
        public string employeeId { get; set; }
        public string email { get; set; }

    }
}