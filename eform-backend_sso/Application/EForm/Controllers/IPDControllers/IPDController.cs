using DataAccess.Models.IPDModel;
using EForm.Authentication;
using EForm.Client;
using EForm.Common;
using EForm.Controllers.BaseControllers;
using EForm.Models.IPDModels;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Cors;
using static EForm.Controllers.IPDControllers.IPDSetupMedicalRecordController;

namespace EForm.Controllers.IPDControllers
{
    [SessionAuthorize]
    [EnableCors(origins: "*", headers: "Content-Type,Origin,X-Auth-Token,Authorization", methods: "get,post")]
    //[CheckLogin]
    public class IPDController : BaseIPDApiController
    {
        [HttpGet]
        [Route("api/IPD/IPD")]
        [Permission(Code = "IIPD1")]
        public IHttpActionResult GetCustomers([FromUri]IPDParameterModel request)
        {
            if (!request.Validate())
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);

            //var user = GetUser();
            //var positions = user.PositionUsers.Where(e => !e.IsDeleted).Select(e => e.Position.EnName);

            return Content(HttpStatusCode.OK, GetIPDForNurse(request));
        }
        [HttpGet]
        [Route("api/IPD/HISDoctor/{id}")]
        [Permission(Code = "IIPD2")]
        public IHttpActionResult HISDoctor(Guid id)
        {
            return Content(HttpStatusCode.OK, new { });
        }
        [HttpGet]
        [Route("api/IPD/HISDoctor/Sync/{id}")]
        [Permission(Code = "IIPD2")]
        public IHttpActionResult SyncHISDoctor(Guid id)
        {
            return Content(HttpStatusCode.OK, new { });
        }
        [HttpPost]
        [CSRFCheck]
        [Route("api/IPD/PrimaryDoctor/{id}")]
        [Permission(Code = "IIPD2")]
        public IHttpActionResult PrimaryDoctor(Guid id, [FromBody] JObject request)
        {
            return Content(HttpStatusCode.BadRequest, new { });
        }
        private dynamic GetIPDForNurse(IPDParameterModel request)
        {
            var site_id = GetSiteId();
            var specialty_id = GetSpecialtyId();
            var query = FilterCustomerForNurse(site_id, specialty_id);

            if (request.Search != null)
            {
                if (request.ConvertedSearch.Length == 8)
                {
                    bool isPID = true;
                    char[] arrChar = request.ConvertedSearch.ToCharArray();
                    foreach (char c in arrChar)
                    {
                        if (c < 48 || c > 57)
                        {
                            isPID = false;
                            break;
                        }
                    }

                    if (isPID)
                    {
                        query = FilterByCustomer(query, $"8{request.ConvertedSearch}");
                    }
                    else
                    {
                        query = FilterByCustomer(query, request.ConvertedSearch);
                    }
                }
                else
                {
                    query = FilterByCustomer(query, request.ConvertedSearch);
                }
            }

            if (request.EmergencyStatus != null)
                query = FilterByEmergencyStatus(query, request.ConvertedEmergencyStatus);

            if (request.User != null)
                query = FilterByUser(query, request.ConvertedUser);

            if (request.StartAdmittedDate != null && request.EndAdmittedDate != null)
                query = FilterByAdmittedDate(query, request.ConvertedStartAdmittedDate, request.ConvertedEndAdmittedDate);
            else if (request.StartAdmittedDate != null)
                query = FilterByStartAdmittedDate(query, request.ConvertedStartAdmittedDate);
            else if (request.EndAdmittedDate != null)
                query = FilterByEndAdmittedDate(query, request.ConvertedEndAdmittedDate);

            if (request.IsDraft == true)
                query = query.Where(e => e.IsDraft);

            var current_user = getUsername();
            if (!IsVIPMANAGE())
                query = query.Where(e => e.UnlockFor != null && (e.UnlockFor == "ALL" || ("," + e.UnlockFor + ",").Contains("," + current_user + ",")));

            int count = query.AsQueryable().Count();
            var timeStartUpdate = GetAppConfig("HIDE_A02_016_050919_VE");
            if (count > 0)
            {
                query = query.AsQueryable().OrderByDescending(m => m.AdmittedDateDate).ThenBy(m => m.EDStatusCreatedAt).ThenByDescending(m => m.AdmittedDate);
                var items = query.Skip((request.PageNumber - 1) * request.PageSize)
                               .Take(request.PageSize)
                               .ToList()
                               .Select(e => DataFormatted(e, timeStartUpdate));
                return new { count, results = items };
            }
            else
            {
                return new { count = 0, results = new Array[] { } };
            }
        }

        private dynamic GetMedicalRecords(Guid ipdId)
        {

            IPD visit = GetIPD(ipdId);
            if (visit == null)
            {
                return Content(HttpStatusCode.BadRequest, Message.IPD_NOT_FOUND);
            }
            var specialtyId = visit.SpecialtyId;
            var medicalRecord = visit.IPDMedicalRecordId;
            var listMedicalRecordIsDeploy = (from m in unitOfWork.IPDSetupMedicalRecordRepository.AsQueryable()
                                             where m.SpecialityId == specialtyId && m.IsDeploy == true && m.FormType == "MedicalRecords"
                                             select new
                                             {
                                                 FormCode = m.Formcode,
                                                 ViName = m.ViName,
                                                 EnName = m.EnName,
                                                 Type = m.Type
                                             }).ToList();



            if (medicalRecord == null)
            {
                if (listMedicalRecordIsDeploy.Count == 0)
                {
                    return null;
                }
                List<dynamic> listMedicalRecords = new List<dynamic>();
                foreach (var item in listMedicalRecordIsDeploy)
                {
                    var part2 = new { FormCode = item.FormCode, ViName = item.ViName, EnName = item.EnName, Type = item.Type + "/Part2", Role = new int[] {1, 3} };

                    var part1 = new { FormCode = item.FormCode, ViName = item.ViName, EnName = item.EnName, Type = item.Type + "/Part1", Role = new int[] { 2 } };

                    var print = new { FormCode = item.FormCode, ViName = item.ViName, EnName = item.EnName, Type = item.Type + "/Print", Role = new int[] { 4 } };

                    listMedicalRecords.Add(part2);
                    listMedicalRecords.Add(part1);
                    listMedicalRecords.Add(print);
                }

                return listMedicalRecords;
            }
            else
            {
                var formMedicalRecord = unitOfWork.IPDMedicalRecordOfPatientRepository.AsQueryable()
                                            .Where(m => m.VisitId == ipdId && !m.IsDeleted)
                                            .Select(m => new
                                            {
                                                FormCode = m.FormCode
                                            }).Distinct().ToList();

                foreach (var item in formMedicalRecord)
                {
                    var setupMedical = (from s in unitOfWork.IPDSetupMedicalRecordRepository.AsQueryable()
                                        where s.SpecialityId == specialtyId && !s.IsDeploy && s.Formcode == item.FormCode && s.FormType == "MedicalRecords"
                                        select new
                                        {
                                            FormCode = s.Formcode,
                                            ViName = s.ViName,
                                            EnName = s.EnName,
                                            Type = s.Type,
                                        }).FirstOrDefault();

                    if (setupMedical != null) listMedicalRecordIsDeploy.Add(setupMedical);
                }
                List<dynamic> listMedicalRecords = new List<dynamic>();
                foreach (var item in listMedicalRecordIsDeploy)
                {
                    var part2 = new { FormCode = item.FormCode, ViName = item.ViName, EnName = item.EnName, Type = item.Type + "/Part2", Role = new int[] { 1, 3 } };

                    var part1 = new { FormCode = item.FormCode, ViName = item.ViName, EnName = item.EnName, Type = item.Type + "/Part1", Role = new int[] { 2 } };

                    var print = new { FormCode = item.FormCode, ViName = item.ViName, EnName = item.EnName, Type = item.Type + "/Print", Role = new int[] { 4 } };

                    listMedicalRecords.Add(part2);
                    listMedicalRecords.Add(part1);
                    listMedicalRecords.Add(print);
                }

                return listMedicalRecords;
            }
        }

        private IQueryable<IPDQueryModel> FilterCustomerForNurse(Guid? site_id, Guid? specialty_id)
        {
            return (from ipd_sql in unitOfWork.IPDRepository.AsQueryable().Where(
                        e => !e.IsDeleted && string.IsNullOrEmpty(e.DeletedBy) && e.CustomerId != null && e.EDStatusId != null &&
                        e.SiteId != null && e.SiteId == site_id &&
                        e.SpecialtyId != null && e.SpecialtyId == specialty_id
                   )
                    join cus_sql in unitOfWork.CustomerRepository.AsQueryable()
                         on ipd_sql.CustomerId equals cus_sql.Id
                    join stt_sql in unitOfWork.EDStatusRepository.AsQueryable()
                        on ipd_sql.EDStatusId equals stt_sql.Id
                    join doc_sql in unitOfWork.UserRepository.AsQueryable()
                        on ipd_sql.PrimaryDoctorId equals doc_sql.Id into dlist
                    from doc_sql in dlist.DefaultIfEmpty()
                    join nur_sql in unitOfWork.UserRepository.AsQueryable()
                        on ipd_sql.PrimaryNurseId equals nur_sql.Id into nlist
                    from nur_sql in nlist.DefaultIfEmpty()
                    select new IPDQueryModel
                    {
                        Id = ipd_sql.Id,
                        VisitCode = ipd_sql.VisitCode,
                        AdmittedDate = ipd_sql.AdmittedDate,
                        PermissionForVisitor = ipd_sql.PermissionForVisitor,
                        AdmittedDateDate = SqlFunctions.DateName("year", ipd_sql.AdmittedDate) +
                                SqlFunctions.StringConvert((double)SqlFunctions.DatePart("month", ipd_sql.AdmittedDate), 2) +
                                SqlFunctions.StringConvert((double)SqlFunctions.DatePart("day", ipd_sql.AdmittedDate), 2),
                        CustomerPID = cus_sql.PID,
                        CustomerPhone = cus_sql.Phone,
                        CustomerFullname = cus_sql.Fullname,
                        CustomerDateOfBirth = cus_sql.DateOfBirth,
                        CustomerIsAllergy = (bool)ipd_sql.IsAllergy,
                        CustomerAllergy = ipd_sql.Allergy,
                        CustomerKindOfAllergy = ipd_sql.KindOfAllergy,
                        EDStatusId = stt_sql.Id,
                        EDStatusCreatedAt = stt_sql.CreatedAt,
                        EDStatusEnName = stt_sql.EnName,
                        EDStatusViName = stt_sql.ViName,
                        PrimaryDoctorId = ipd_sql.PrimaryDoctorId,
                        PrimaryDoctorUsername = doc_sql.Username,
                        PrimaryNurseId = ipd_sql.PrimaryNurseId,
                        PrimaryNurseUsername = nur_sql.Username,
                        UnlockFor = ipd_sql.UnlockFor,
                        IsVip = cus_sql.IsVip,
                        IsDraft = ipd_sql.IsDraft,
                        CreatedAt = ipd_sql.CreatedAt
                    });
        }

        private IQueryable<IPDQueryModel> FilterByCustomer(IQueryable<IPDQueryModel> query, string search)
        {
            return query.Where(e =>
                (e.CustomerPID != null && e.CustomerPID == search)
                || (e.CustomerFullname != null && e.CustomerFullname.ToLower().Contains(search))
                || (e.CustomerPhone != null && e.CustomerPhone.Contains(search))
            );
        }

        private IQueryable<IPDQueryModel> FilterByEmergencyStatus(IQueryable<IPDQueryModel> query, List<Guid?> search)
        {
            return query.Where(e => search.Contains(e.EDStatusId));
        }

        private IQueryable<IPDQueryModel> FilterByStartAdmittedDate(IQueryable<IPDQueryModel> query, DateTime? search)
        {
            return query.Where(e => e.AdmittedDate != null && e.AdmittedDate >= search);
        }
        private IQueryable<IPDQueryModel> FilterByEndAdmittedDate(IQueryable<IPDQueryModel> query, DateTime? search)
        {
            return query.Where(e => e.AdmittedDate != null && e.AdmittedDate <= search);
        }
        private IQueryable<IPDQueryModel> FilterByAdmittedDate(IQueryable<IPDQueryModel> query, DateTime? start_date, DateTime? end_date)
        {
            return query.Where(e => e.AdmittedDate != null && e.AdmittedDate >= start_date && e.AdmittedDate <= end_date);
        }

        private IQueryable<IPDQueryModel> FilterByUser(IQueryable<IPDQueryModel> query, List<Guid?> search)
        {
            return query.Where(e => search.Contains(e.PrimaryDoctorId) || search.Contains(e.PrimaryNurseId));
        }

        private dynamic DataFormatted(IPDQueryModel visit, string timeStartUpdate)
        {
            return new
            {
                visit.Id,
                Customer = new
                {
                    PID = visit.CustomerPID,
                    Phone = visit.CustomerPhone,
                    Fullname = visit.CustomerFullname,
                    DateOfBirth = visit.CustomerDateOfBirth?.ToString(Constant.DATE_FORMAT),
                    IsAllergy = visit.CustomerIsAllergy,
                    Allergy = visit.CustomerAllergy,
                    KindOfAllergy = visit.CustomerKindOfAllergy,
                    visit.IsVip
                },
                PrimaryDoctor = visit.PrimaryDoctorUsername,
                Nurse = visit.PrimaryNurseUsername,
                visit.PermissionForVisitor,
                visit.VisitCode,
                AdmittedDate = visit.AdmittedDate.ToString(Constant.DATE_TIME_FORMAT_WITHOUT_SECOND),
                EmergencyStatus = new { EnName = visit.EDStatusEnName, ViName = visit.EDStatusViName },
                VisitAllergy = getIPDAllergyModel(visit, visit.Id),
                MedicalRecordType = GetMedicalRecords(visit.Id),
                visit.IsDraft,
                HideFormNewborn = IsForNeonatalMaternityV2((DateTime)visit.CreatedAt, timeStartUpdate)
            };
        }
    }
}
