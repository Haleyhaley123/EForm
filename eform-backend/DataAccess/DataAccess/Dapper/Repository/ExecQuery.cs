using Dapper;
using DataAccess.Models;
using DataAccess.Models.GeneralModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Dapper.Repository
{
    public class ExecQuery
    {
        private static IDbConnection Connection
        {
            get
            {
                string connectionString = ConfigurationManager.ConnectionStrings["EmergencyDepartmentContext"].ConnectionString;
                return new SqlConnection(connectionString);
            }
        }
        //public static Customer getEd(Guid visitId)
        //{
        //    using (IDbConnection cn = Connection)
        //    {
        //        //string sqlCustomer = "SELECT Id,PIDEhos FROM Customers WHERE IsDeleted =  0 AND PID IS NOT NULL AND PID = @pid";
        //        //Customer customer = cn.SqlQ<Customer>(sqlCustomer, new { pid }).FirstOrDefault();
        //        //return customer;
        //    }
        //}
        public static List<User> getUsersWithPositionName(string EnName)
        {
            using (IDbConnection cn = Connection)
            {
                string sql = "select Distinct u.Username from Positions p  inner join PositionUsers pu on pu.PositionId = p.Id inner join Users u on pu.UserId = u.Id  where p.EnName = @EnName and u.IsDeleted = 0 and pu.IsDeleted = 0 and p.IsDeleted = 0";
                List<User> users = cn.Query<User>(sql, new { EnName }).ToList();
                return users;
            }
        }
        public static Customer getCustomerByPId(string pid)
        {
            using (IDbConnection cn = Connection)
            {
                string sqlCustomer = "SELECT Id,PIDEhos,DateOfBirth,Gender FROM Customers WHERE IsDeleted =  0 AND PID IS NOT NULL AND PID = @pid";
                Customer customer = cn.Query<Customer>(sqlCustomer, new { pid }).FirstOrDefault();
                return customer;
            }
        }
        public static List<InfoRecord> getMedicalRecordByCustomerId(Guid customerId)
        {
            using (IDbConnection cn = Connection)
            {

                List<InfoRecord> records = new List<InfoRecord>();
                string sqlEd = "SELECT DISTINCT ed.AdmittedDate, ed.DischargeDate ,spec.Code AS SpecialtyCode, site.Code AS SiteCode, spec.Id AS SpecialtyId, site.ApiCode AS SpecialtyApiCode, spec.EnName AS SpecialtyEnName, spec.ViName AS SpecialtyViName, site.[Name] AS SpecialtySite, site.ApiCode AS SpecialtySiteApiCode, site.EnName AS SpecialtySiteEnName, site.ViName AS SpecialtySiteViName, site.[Name] AS SpecialtySiteSite, status.EnName AS StatusEnName, status.Code AS StatusCode,status.Id AS StatusId,ed.VisitCode,ed.RecordCode,ed.AdmittedDate  AS ExaminationTime , (ISNULL(doctor.EHOSAccount,'') + ed.VisitCode)  AS EHOSVisitCode , status.ViName AS StatusViName, 'ED' AS [Type], ( spec.ViName + ' - ' + site.Name ) as Name, doctorPr.Username  AS DoctorUsername , nurse.Username  AS NurseUsername, ed.Id FROM EDs ed LEFT JOIN Specialties spec ON ed.SpecialtyId = spec.Id LEFT JOIN Sites site ON spec.SiteId = site.Id LEFT JOIN EDStatus status ON ed.EDStatusId = status.Id LEFT JOIN EDDischargeInformations di ON ed.DischargeInformationId = di.Id LEFT JOIN Users doctor ON  di.UpdatedBy  = doctor.Username LEFT JOIN Users doctorPr ON  ed.PrimaryDoctorId  = doctorPr.Id LEFT JOIN Users nurse ON  ed.CurrentNurseId  = nurse.Id WHERE CustomerId = @customerId and  ed.IsDeleted = 0";
                List<InfoRecord> recordEds = cn.Query<InfoRecord>(sqlEd, new { customerId }).ToList();
                if (recordEds.Count > 0)
                {
                    records.AddRange(recordEds);
                }
                string sqlOpd = "SELECT DISTINCT opd.IsAnesthesia AS IsPreAnesthesia, opd.AdmittedDate, opd.DischargeDate, spec.Code AS SpecialtyCode, site.Code AS SiteCode, spec.Id AS SpecialtyId, site.ApiCode AS SpecialtyApiCode, spec.EnName AS SpecialtyEnName, spec.ViName AS SpecialtyViName, site.[Name] AS SpecialtySite, site.ApiCode AS SpecialtySiteApiCode, site.EnName AS SpecialtySiteEnName, site.ViName AS SpecialtySiteViName, site.[Name] AS SpecialtySiteSite, status.EnName AS StatusEnName, status.Code AS StatusCode,status.Id AS StatusId,opd.VisitCode,opd.RecordCode,opd.AdmittedDate  AS ExaminationTime , (ISNULL(doctor.EHOSAccount,'') + opd.VisitCode) AS EHOSVisitCode , status.ViName AS StatusViName, 'OPD' AS [Type], ( spec.ViName + ' - ' + site.Name ) as Name, doctor.Username  AS DoctorUsername , nurse.Username  AS NurseUsername, opd.Id FROM OPDs opd LEFT JOIN Specialties spec ON opd.SpecialtyId = spec.Id LEFT JOIN Sites site ON spec.SiteId = site.Id LEFT JOIN EDStatus status ON opd.EDStatusId = status.Id LEFT JOIN OPDOutpatientExaminationNotes di ON opd.OPDOutpatientExaminationNoteId = di.Id LEFT JOIN Users doctor ON  opd.PrimaryDoctorId  = doctor.Id LEFT JOIN Users nurse ON  opd.PrimaryNurseId  = nurse.Id WHERE  CustomerId = @customerId and opd.IsDeleted = 0";
                List<InfoRecord> recordsOpds = cn.Query<InfoRecord>(sqlOpd, new { customerId }).ToList();
                if (recordsOpds.Count > 0)
                {
                    records.AddRange(recordsOpds);
                }
                string sqlIpd = "SELECT DISTINCT  ipd.AdmittedDate, ipd.DischargeDate, spec.Code AS SpecialtyCode, site.Code AS SiteCode, spec.Id AS SpecialtyId, site.ApiCode AS SpecialtyApiCode, spec.EnName AS SpecialtyEnName, spec.ViName AS SpecialtyViName, site.[Name] AS SpecialtySite, site.ApiCode AS SpecialtySiteApiCode, site.EnName AS SpecialtySiteEnName, site.ViName AS SpecialtySiteViName, site.[Name] AS SpecialtySiteSite, status.EnName AS StatusEnName, status.Code AS StatusCode,status.Id AS StatusId,ipd.VisitCode,ipd.RecordCode,ipd.AdmittedDate  AS ExaminationTime , (ISNULL(doctor.EHOSAccount,'') + ipd.VisitCode ) AS EHOSVisitCode , status.ViName AS StatusViName, 'IPD' AS [Type], ( spec.ViName + ' - ' + site.Name ) as Name, doctor.Username DoctorUsername , nurse.Username  AS NurseUsername, ipd.Id FROM Ipds ipd LEFT JOIN Specialties spec ON ipd.SpecialtyId = spec.Id LEFT JOIN Sites site ON spec.SiteId = site.Id LEFT JOIN EDStatus status ON ipd.EDStatusId = status.Id LEFT JOIN IPDMedicalRecords mere ON ipd.IPDMedicalRecordId = mere.Id LEFT JOIN IPDMedicalRecordPart2 di ON mere.IPDMedicalRecordPart2Id = di.Id LEFT JOIN Users doctor ON  ipd.PrimaryDoctorId  = doctor.Id LEFT JOIN Users nurse ON  ipd.PrimaryNurseId  = nurse.Id WHERE CustomerId = @customerId and  ipd.IsDeleted = 0";
                List<InfoRecord> recordIpds = cn.Query<InfoRecord>(sqlIpd, new { customerId }).ToList();
                if (recordIpds.Count > 0)
                {
                    records.AddRange(recordIpds);
                }
                string sqlEoc = "SELECT DISTINCT eoc.AdmittedDate, eoc.DischargeDate, spec.Code AS SpecialtyCode, site.Code AS SiteCode, spec.Id AS SpecialtyId, site.ApiCode AS SpecialtyApiCode, spec.EnName AS SpecialtyEnName, spec.ViName AS SpecialtyViName, site.[Name] AS SpecialtySite, site.ApiCode AS SpecialtySiteApiCode, site.EnName AS SpecialtySiteEnName, site.ViName AS SpecialtySiteViName, site.[Name] AS SpecialtySiteSite, status.EnName AS StatusEnName, status.Code AS StatusCode,status.Id AS StatusId,eoc.VisitCode,eoc.RecordCode,eoc.AdmittedDate  AS ExaminationTime , (ISNULL(doctor.EHOSAccount,'') + eoc.VisitCode ) AS EHOSVisitCode , status.ViName AS StatusViName, 'EOC' AS [Type], ( spec.ViName + ' - ' + site.Name ) as Name, doctor.Username DoctorUsername , nurse.Username  AS NurseUsername, eoc.Id FROM EOCs eoc LEFT JOIN Specialties spec ON eoc.SpecialtyId = spec.Id LEFT JOIN Sites site ON spec.SiteId = site.Id LEFT JOIN EDStatus status ON eoc.StatusId = status.Id LEFT JOIN Users doctor ON  eoc.PrimaryDoctorId  = doctor.Id LEFT JOIN Users nurse ON  eoc.PrimaryNurseId  = nurse.Id WHERE CustomerId = @customerId and  eoc.IsDeleted = 0";
                List<InfoRecord> recordEoc = cn.Query<InfoRecord>(sqlEoc, new { customerId }).ToList();
                if (recordEoc.Count > 0)
                {
                    records.AddRange(recordEoc);
                }
                return records;
            }
        }
        public static List<DataModel> getValueAndNodeInMaster(Guid visitId, string toLanguage, string form)
        {
            using (IDbConnection cn = Connection)
            {
                string sqlMaster = "select td.Value,m.Note from Translations t inner join TranslationDatas td on td.TranslationId = t.Id inner join MasterDatas m on m.Code = td.Code where t.VisitId = @visitId and t.IsDeleted = 0 and t.ToLanguage = @toLanguage and td.IsDeleted = 0 and m.Form = @form and m.Level = 2 and m.Note <> ''order by td.UpdatedAt desc";
                List<DataModel> customer = cn.Query<DataModel>(sqlMaster, new { visitId, toLanguage, form }).ToList();
                return customer;
            }
        }
        public static List<ChargeItem> getChargeItemById(Guid Id)
        {
            using (IDbConnection cn = Connection)
            {
                string sql = "select * from ChargeItems where ChargeItemType IN ('Lab','Rad','Allies') and ChargeId = @Id";
                List<ChargeItem> customer = cn.Query<ChargeItem>(sql, new { Id }).ToList();
                return customer;
            }
        }
        public static List<ChargeItem> getChargeItemByArrayId(Guid[] chargeItemId)
        {
            List<ChargeItem> chargeItems = new List<ChargeItem>();
            if (chargeItemId.Length > 0)
            {
                using (IDbConnection cn = Connection)
                {
                    string sql = "select * from ChargeItems where Id IN @chargeItemId";
                    chargeItems = cn.Query<ChargeItem>(sql, new { @chargeItemId = chargeItemId }).ToList();
                }
            }
            return chargeItems;
            
        }
        public static void updateChargeItem(List<ChargeItemDto> items)
        {
           
            if (items.Count > 0)
            {
                var username = GetUserName();
                for (int i = 0; i < items.Count; i++)
                {
                    items[i].UpdatedBy = username;
                    items[i].UpdatedAt = DateTime.Now;
                }
                using (IDbConnection cn = Connection)
                {
                    cn.Updates("ChargeItems", items);
                }
            }
        }
        public static Charge getChargeById(Guid Id)
        {
            using (IDbConnection cn = Connection)
            {
                string sql = "select * from Charges where Id = @Id";
                Charge charge = cn.Query<Charge>(sql, new { Id }).FirstOrDefault();
                return charge;
            }
        }
        public static void updateCharge(ChargeDto c)
        {
            using (IDbConnection cn = Connection)
            {
                cn.Update("Charges", c);
            }
        }
        //public static TranslationDto getTranslation(Guid id)
        //{
        //    using (IDbConnection cn = Connection)
        //    {
        //        string sqlTranslation = "select Id,VisitTypeGroupCode,VisitId,EnName,UpdatedAt from Translations where Id = @id and IsDeleted = 0";
        //        TranslationDto result = cn.Query<TranslationDto>(sqlTranslation, new { id }).FirstOrDefault();
        //        return result;
        //    }
        //}
        //public static List<TranslationDataDto> getTranslationDatas(Guid translationId)
        //{
        //    using (IDbConnection cn = Connection)
        //    {
        //        string sqlTranslationData = "select Code,Value from TranslationDatas  where TranslationId = @translationId and IsDeleted = 0";
        //        List<TranslationDataDto> results = cn.Query<TranslationDataDto>(sqlTranslationData, new { translationId }).ToList();
        //        return results;
        //    }
        //}
        private static string GetUserName()
        {
            try
            {
                var claims = ClaimsPrincipal.Current.Identities.First().Claims.ToList();
                return claims?.FirstOrDefault(x => x.Type.Equals(ClaimTypes.Name, StringComparison.OrdinalIgnoreCase))?.Value;
            }
            catch (Exception)
            {
                return "";
            }
        }
    }

}
