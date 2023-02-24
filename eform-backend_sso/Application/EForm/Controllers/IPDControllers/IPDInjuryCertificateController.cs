using DataAccess.Models;
using DataAccess.Models.IPDModel;
using EForm.Authentication;
using EForm.Common;
using EForm.Controllers.BaseControllers;
using EForm.Models;
using EForm.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace EForm.Controllers.IPDControllers
{
    [SessionAuthorize]
    public class IPDInjuryCertificateController : BaseIPDApiController
    {
        [HttpGet]
        [Route("api/IPD/Document/InjuryCertificate/{visitId}")]
        [Permission(Code = "IPDINCERT1")]
        public IHttpActionResult CreateInjuryCertificateAPI(Guid visitId)
        {
            var ipdInfo = GetIPD(visitId);
            if (ipdInfo == null)
            {
                return Content(HttpStatusCode.NotFound, Message.IPD_NOT_FOUND);
            }

           var yesValue = ipdInfo.IPDMedicalRecord?.IPDMedicalRecordPart3?.IPDMedicalRecordPart3Datas?.FirstOrDefault(x => x.Code == "IPDMRPECOINYES")?.Value;
            if (string.IsNullOrEmpty(yesValue) || Convert.ToBoolean(yesValue) == false)
            {
                return Content(HttpStatusCode.NotFound, Message.PATIENT_NOT_CONFIRM_INJURY);
            }

            var injuryCertificate = GetOrCreateInjuryCertificate(ipdInfo.Id);
            var result = GetDetailInjuryCertificate(injuryCertificate, ipdInfo);

            return Content(HttpStatusCode.OK, result);
        }

        [HttpPost]
        [CSRFCheck]
        [Route("api/IPD/Document/InjuryCertificate/Confirm/{visitId}")]
        [Permission(Code = "IPDINCERT2")]
        public IHttpActionResult ConfirmEDInjuryCertificateAPI(Guid visitId, [FromBody] JObject request)
        {
            var ipdInfo = GetIPD(visitId);
            if (ipdInfo == null)
            {
                return Content(HttpStatusCode.NotFound, Message.IPD_NOT_FOUND);
            }

            var injuryCertificate = GetOrCreateInjuryCertificate(ipdInfo.Id);
            if (injuryCertificate == null)
            {
                return Content(HttpStatusCode.NotFound, Message.IPD_NOT_FOUND);
            }

            var username = request["username"]?.ToString();
            var password = request["password"]?.ToString();
            var kind = request["kind"]?.ToString();

            var user = GetAcceptUser(username, password);
            if (user == null)
            {
                return Content(HttpStatusCode.BadRequest, Message.INFO_INCORRECT);
            }

            var positions = user.PositionUsers.Where(e => !e.IsDeleted).Select(e => e.Position.EnName);

            if (kind == "Doctor" && positions.Contains("Doctor") && injuryCertificate.DoctorId == null)
            {
                injuryCertificate.DoctorId = user.Id;
                injuryCertificate.DoctorTime = DateTime.Now;
            }
            else if (kind == "HeadOfDept" && positions.Contains("Head Of Department") && injuryCertificate.HeadOfDeptId == null)
            {
                injuryCertificate.HeadOfDeptId = user.Id;
                injuryCertificate.HeadOfDeptTime = DateTime.Now;
            }
            else if (kind == "Director" && positions.Contains("Director") && injuryCertificate.DirectorId == null)
            {
                injuryCertificate.DirectorId = user.Id;
                injuryCertificate.DirectorTime = DateTime.Now;
            }
            else
            {
                return Content(HttpStatusCode.BadRequest, Message.OWNER_FORBIDDEN);
            }

            unitOfWork.IPDInjuryCertificateRepository.Update(injuryCertificate);
            unitOfWork.Commit();
            return Content(HttpStatusCode.OK, Message.SUCCESS);
        }

        private IPDInjuryCertificate GetOrCreateInjuryCertificate(Guid visitId)
        {
            IPDInjuryCertificate injuryCertificate = new IPDInjuryCertificate();
            if (unitOfWork.IPDInjuryCertificateRepository != null)
            {
                injuryCertificate = unitOfWork.IPDInjuryCertificateRepository
                                                .FirstOrDefault(e => !e.IsDeleted && e.VisitId != null && e.VisitId == visitId);
            }

            if (injuryCertificate == null)
            {
                injuryCertificate = new IPDInjuryCertificate()
                {
                    VisitId = visitId
                };
                unitOfWork.IPDInjuryCertificateRepository.Add(injuryCertificate);
                unitOfWork.Commit();
            }
            return injuryCertificate;
        }

        private dynamic GetDetailInjuryCertificate(IPDInjuryCertificate ic, IPD ipd)
        {
            var customer = ipd.Customer;
            var gender = new CustomerUtil(customer).GetGender();
            var first_ipd = GetFirstIpd(ipd);
            var admitted_date = first_ipd.CurrentDate;

            var medicalRecord = ipd.IPDMedicalRecord;

            bool IsLocked = IPDIsBlock(ipd, Constant.IPDFormCode.IPDGiayChungNhanThuongTich);


            string presentComplaint = "";
            string diagnosis = "";
            string treatmentMethod = "";
            string dischargeStatus = "";
            //tinh trang nhap vien
            var hospitalized = string.Empty;

            if (medicalRecord != null)
            {
                var medicalRecordPart2 = medicalRecord.IPDMedicalRecordPart2;
                if (medicalRecordPart2 != null)
                {
                    var medicalRecordPart2Datas = medicalRecordPart2.IPDMedicalRecordPart2Datas;
                    if (medicalRecordPart2Datas != null)
                    {
                        //ly do nhap vien
                        presentComplaint = medicalRecordPart2Datas.FirstOrDefault(e => !e.IsDeleted && e.Code == "IPDMRPTLDVVANS")?.Value;

                        string tomTatBenhAn = medicalRecordPart2Datas.FirstOrDefault(e => e.Code == "IPDMRPTTTBAANS")?.Value ?? string.Empty;
                        //string abc = medicalRecordPart2Datas.FirstOrDefault(e => e.Code == "IPDMRPT894")?.Value;
                        if (tomTatBenhAn != "")
                        {
                            tomTatBenhAn = $"{tomTatBenhAn}";
                        }

                        hospitalized = $"{tomTatBenhAn}";
                    }
                }

                var medicalRecordPart3 = medicalRecord.IPDMedicalRecordPart3;
                if (medicalRecordPart3 != null)
                {
                    var medicalRecordPart3Datas = medicalRecordPart3.IPDMedicalRecordPart3Datas;
                    if (medicalRecordPart3Datas != null)
                    {
                        //chan doan
                        string mainDiagnosis = medicalRecordPart3Datas.FirstOrDefault(e => !e.IsDeleted && e.Code == "IPDMRPECDBCANS")?.Value;
                        string extDiagnosis = medicalRecordPart3Datas.FirstOrDefault(e => !e.IsDeleted && e.Code == "IPDMRPECDKTANS")?.Value;

                        string mainIDC10 = string.Empty;
                        var objMainICD10 = medicalRecordPart3Datas.FirstOrDefault(e => !e.IsDeleted && e.Code == "IPDMRPEICDCANS")?.Value;
                        if (!string.IsNullOrEmpty(objMainICD10) && objMainICD10 != "null" && objMainICD10 != "\"\"")
                        {
                            var lstMainIDC10 = JsonConvert.DeserializeObject<List<MasterDataValue>>(objMainICD10);
                            mainIDC10 = lstMainIDC10.Count > 0 ? lstMainIDC10[0].Code : string.Empty;
                        }

                        string extIDC10 = string.Empty;
                        var objExtICD10 = medicalRecordPart3Datas.FirstOrDefault(e => !e.IsDeleted && e.Code == "IPDMRPEICDPANS")?.Value;
                        if (!string.IsNullOrEmpty(objExtICD10) && objExtICD10 != "null" && objExtICD10 != "\"\"")
                        {
                            var lstExtIDC10 = JsonConvert.DeserializeObject<List<MasterDataValue>>(objExtICD10);
                            extIDC10 = lstExtIDC10.Count > 0 ? string.Join("; ", lstExtIDC10.Select(e => e.Code).ToArray()) : string.Empty;
                        }

                        //chan doan
                        diagnosis = mainDiagnosis + (extDiagnosis == string.Empty || extDiagnosis == null ? string.Empty : "/ " + extDiagnosis) +
                                  (mainIDC10 == string.Empty && extIDC10 == string.Empty ? string.Empty : " (" + mainIDC10 + (extIDC10.Trim() != string.Empty ? "/ " + extIDC10 : string.Empty) + ")");

                        //phuong phap dieu tri
                        treatmentMethod = GetTreatmentsLast(ipd.Id, medicalRecordPart3.Id, medicalRecordPart3Datas.ToList()); 
                        //Tinh trang nguoi benh ra vien
                        dischargeStatus = medicalRecordPart3Datas.FirstOrDefault(e => !e.IsDeleted && e.Code == "IPDMRPETTNBANS")?.Value;
                    }
                }
            }


            User doctor = new User();
            User headOfDept = new User();
            User director = new User();
            if (ic != null)
            {
                doctor = ic.Doctor;
                headOfDept = ic.HeadOfDept;
                director = ic.Director;
            }

            DateTime DischargeDate = new DateTime();
            if (ipd.DischargeDate != null)
            {
                DischargeDate = (DateTime)ipd.DischargeDate;
            }
            var site = GetSite();
            var translation_util = new TranslationUtil(unitOfWork, ipd.Id, "IPD", "Injury Certificate");
            var translations = translation_util.GetList();

            return new
            {
                Location = site?.Location,
                SiteName = site?.Name,
                Province = site?.Province,
                LocationUnit = site?.LocationUnit,

                CustomerId = customer.PID,
                CustomerName = customer.Fullname,
                DateOfBirth = customer.DateOfBirth?.ToString(Constant.DATE_FORMAT),
                Gender = gender,
                Job = customer.Job,
                MOHJob = customer.MOHJob,
                WorkPlace = customer.WorkPlace,
                IdCard = customer.IdentificationCard,
                IssueDate = customer.IssueDate?.ToString(Constant.DATE_FORMAT),
                IssuePlace = customer.IssuePlace,
                Address = (customer.MOHAddress?.Trim() == string.Empty || customer.MOHAddress?.Trim() == null ? string.Empty : customer.MOHAddress?.Trim()) +
                          (customer.MOHDistrict?.Trim() == string.Empty || customer.MOHDistrict?.Trim() == null ? string.Empty : " - " + customer.MOHDistrict?.Trim()) +
                          (customer.MOHProvince?.Trim() == string.Empty || customer.MOHProvince?.Trim() == null ? string.Empty : " - " + customer.MOHProvince?.Trim()),

                AdmittedDate = admitted_date,
                DischargeDate = DischargeDate.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND),
                PresentComplain = presentComplaint,
                Diagnosis = diagnosis,
                TreatmentProcedures = treatmentMethod,
                HospitalizedStatus = hospitalized,
                DischargeStatus = dischargeStatus,

                DoctorTime = ic.DoctorTime?.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND),
                Doctor = new { doctor?.Username, doctor?.Fullname, doctor?.DisplayName, doctor?.Title },
                HeadOfDeptTime = ic.HeadOfDeptTime?.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND),
                HeadOfDept = new { headOfDept?.Username, headOfDept?.Fullname, headOfDept?.DisplayName, headOfDept?.Title },
                DirectorTime = ic.DirectorTime?.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND),
                Director = new { director?.Username, director?.Fullname, director?.DisplayName, director?.Title },
                IsLocked,
                Translations = translations
            };
        }
        public string GetTreatmentsAndProceduresUB(Guid part3_id)
        {
            string value = "";
            var part3Data = unitOfWork.IPDMedicalRecordPart3DataRepository.AsQueryable().Where(x => x.IPDMedicalRecordPart3Id == part3_id);
            var data = (from part3 in part3Data
                        join m in unitOfWork.MasterDataRepository.AsQueryable()
                          on part3.Code equals m.Code
                        select new
                        {
                            Code = part3.Code,
                            Value = part3.Value,
                            ViName = m.ViName,
                        }).ToList();
            var DD = data.Where(x => (x.Code == "IPDMRPE1002" || x.Code == "IPDMRPE1003") && x.Value.ToUpper() == "TRUE").ToList();
            if (DD.Count == 1)
            {

                value += DD[0].ViName.ToString().Contains(":") ? DD[0].ViName.ToString().Trim() + "\n" : DD[0].ViName.ToString().Trim() + ":\n";
            }
            string[] codes1 = new string[] { "IPDMRPE1005", "IPDMRPE1009", "IPDMRPE1013", "IPDMRPE1015", "IPDMRPE1019", "IPDMRPE1021" };
            foreach (var code in codes1)
            {
                var obj = data.FirstOrDefault(x => x.Code == code);
                if (obj != null)
                {
                    switch (code)
                    {
                        case "IPDMRPE1005":
                            var th = data.FirstOrDefault(x => x.Code == "IPDMRPE1007");
                            if (!string.IsNullOrEmpty(obj.Value) || !string.IsNullOrEmpty(th.Value))
                            {
                                value += obj.ViName.ToString().Contains(":") ? obj.ViName.ToString().Trim() + "&emsp;&emsp;tại u: " + obj.Value.Trim() + " Gy&emsp;&emsp;" : obj.ViName.ToString().Trim() + "&emsp;&emsp;tại u: " + obj.Value.Trim() + " Gy&emsp;&emsp;";
                                value += th.ViName.ToString().Contains(":") ? th.ViName.ToString().Trim() + " " + th.Value.Trim() + "\n" : th.ViName.ToString().Trim() + ": " + th.Value.Trim() + "\n";
                            }
                            break;
                        case "IPDMRPE1009":
                            th = data.FirstOrDefault(x => x.Code == "IPDMRPE1011");
                            if (!string.IsNullOrEmpty(obj.Value) || !string.IsNullOrEmpty(th.Value))
                            {
                                value += obj.ViName.ToString().Contains(":") ? obj.ViName.ToString().Trim() + "&emsp;&emsp;tại u: " + obj.Value.Trim() + " Gy&emsp;&emsp;" : obj.ViName.ToString().Trim() + "&emsp;&emsp;tại u: " + obj.Value.Trim() + " Gy&emsp;&emsp;";
                                value += th.ViName.ToString().Contains(":") ? th.ViName.ToString().Trim() + " " + th.Value.Trim() + "\n" : th.ViName.ToString().Trim() + ": " + th.Value.Trim() + "\n";
                            }
                            break;
                        case "IPDMRPE1015":
                            th = data.FirstOrDefault(x => x.Code == "IPDMRPE1017");
                            if (!string.IsNullOrEmpty(obj.Value) || !string.IsNullOrEmpty(th.Value))
                            {
                                value += obj.ViName.ToString().Contains(":") ? obj.ViName.ToString().Trim() + "&emsp;&emsp;tại u: " + obj.Value.Trim() + " Gy&emsp;&emsp;" : obj.ViName.ToString().Trim() + "&emsp;&emsp;tại u: " + obj.Value.Trim() + " Gy&emsp;&emsp;";
                                value += th.ViName.ToString().Contains(":") ? th.ViName.ToString().Trim() + " " + th.Value.Trim() + "\n" : th.ViName.ToString().Trim() + ": " + th.Value.Trim() + "\n";
                            }
                            break;
                        default:
                            if (!string.IsNullOrEmpty(obj.Value))
                            {
                                value += obj.ViName.ToString().Contains(":") ? obj.ViName.ToString().Trim() + " " + obj.Value.Trim() + "\n" : obj.ViName.ToString().Trim() + ": " + obj.Value.Trim() + "\n";
                            }
                            break;
                    }

                    //else
                    //{
                    //    value += obj.ViName.ToString().Contains(":") ? obj.ViName.ToString().Trim() + "&emsp;&emsp;tại u:&emsp;&emsp;" : obj.ViName.ToString().Trim() + ":&emsp;&emsp;tại u:&emsp;&emsp;";
                    //}

                }
            }
            var DAPUNG = data.Where(x => (x.Code == "IPDMRPE1023" || x.Code == "IPDMRPE1024" || x.Code == "IPDMRPE1025") && x.Value.ToUpper() == "TRUE").ToList();
            if (DAPUNG.Count == 1)
            {

                if (!string.IsNullOrEmpty(DAPUNG[0].ViName))
                {
                    var dapungText = data.FirstOrDefault(x => x.Code == "IPDMRPE1026");
                    value += dapungText.ViName.ToString().Contains(":") ? dapungText.ViName.ToString().Trim() + " " : dapungText.ViName.ToString().Trim() + ": ";
                    value += DAPUNG[0].ViName.ToString().Contains(":") ? DAPUNG[0].ViName.ToString().Trim() : DAPUNG[0].ViName.ToString().Trim() + ": ";
                    value += dapungText?.Value.ToString().Trim() + "\n";
                }

            }
            string[] codes2 = new string[] { "IPDMRPE1028" };
            foreach (var code in codes2)
            {
                var obj = data.FirstOrDefault(x => x.Code == code);
                if (!string.IsNullOrEmpty(obj.Value))
                {
                    value += obj.ViName.ToString().Contains(":") ? obj.ViName.ToString().Trim() + " " + obj.Value + "\n" : obj.ViName.ToString().Trim() + ": " + obj.Value.ToString().Trim() + "\n";
                }

            }
            return value;
        }
    }
}