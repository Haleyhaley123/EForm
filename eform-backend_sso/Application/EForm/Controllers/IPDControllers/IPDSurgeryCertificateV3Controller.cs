using DataAccess.Models;
using DataAccess.Models.EIOModel;
using DataAccess.Models.IPDModel;
using EForm.Authentication;
using EForm.Common;
using EForm.Controllers.BaseControllers.BaseEIOControllers;
using EForm.Models;
using EForm.Models.IPDModels;
using EForm.Models.PrescriptionModels;
using EForm.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace EForm.Controllers.IPDControllers
{
    [SessionAuthorize]
    public class IPDSurgeryCertificateV3Controller : EIOProcedureSummaryController
    {
        [HttpGet]
        [Route("api/IPD/SurgeryCertificateV3/DetailById/{visitId}/{formId}")]
        [Permission(Code = "IPDSURCER03")]
        public IHttpActionResult GetDetailSurgeryCertificateAPI(Guid visitId, Guid formId)
        {
            var ipd = GetIPD(visitId);
            if (ipd == null)
                return Content(HttpStatusCode.NotFound, Message.IPD_NOT_FOUND);

            IPDSurgeryCertificate certificate = GetSurgeryCertificateById(formId);
            if (certificate == null)
                return Content(HttpStatusCode.BadRequest, new
                {
                    Message.EIO_PRSU_NOT_FOUND,
                    IsLocked = IPDIsBlock(ipd, Constant.IPDFormCode.IPDSurgeryCertificate),
                });

            var doctor = certificate.ProcedureDoctor; 
            var dean = certificate.Dean; 
            var director = certificate.Director;            

            var data = certificate.IPDSurgeryCertificateDatas.Where(e => !e.IsDeleted)
                .Select(e => new MappingData() { Code = e.Code, Value = e.Value }).OrderBy(e => e.Code).ToList();           
            if (certificate.FormId != null)
            {
                
                    Dictionary<string, string> codesv3 = new Dictionary<string, string>()
                        {
                            {"IPDSURCER08", "SAPSNEW6"},
                            {"IPDSURCER10", "SAPSNEW4"},
                            {"IPDSURCER04", "SAPSNEW2"},
                            {"IPDSURCER22", "SAPSNEW10"},
                            {"IPDSURCER12", "SAPSNEW8"},
                            {"IPDSURCER14", "SAPSNEW12"},
                            {"IPDSURCER16", "SAPSNEW26"},
                            {"IPDSURCER18", "SAPSNEW28"},
                            {"IPDSURCER20", "SAPSNEW32"},

                     };
                Dictionary<string, string> codesv4 = new Dictionary<string, string>()
                        {
                            {"IPDSURCER08", "SSNEW6"},
                            {"IPDSURCER10", "SSNEW4"},
                            {"IPDSURCER04", "SSNEW2"},
                            {"IPDSURCER22", "SSNEW10"},
                            {"IPDSURCER12", "SSNEW8"},
                            {"IPDSURCER14", "SSNEW12"},
                            {"IPDSURCER16", "SSNEW26"},
                            {"IPDSURCER18", "SSNEW28"},
                            {"IPDSURCER20", "SSNEW32"},

                     };


                Guid? procedureId = certificate.FormId;
                var procedure = GetProcedureSummary(procedureId);
                if(certificate.Version == "3")
                {
                    data = AutofillFromProcedure(data, procedure, codesv3);
                }
                if (certificate.Version == "4")
                {
                    data = AutofillFromProcedure(data, procedure, codesv4);
                }
                data = FormatString(data);
            }

            string ngayVaoVien = "";
            var first_visit = GetFirstIpd(ipd);
            if (first_visit.CurrentType == "OPD")
                ngayVaoVien = first_visit.TransferDate;

            ngayVaoVien = first_visit.CurrentDate;
            string ngayRaVien = "";
            
            var medicalRecord = ipd.IPDMedicalRecord;
            switch (ipd.EDStatus.ViName)
            {
                case "Ra viện":
                    if (medicalRecord != null)
                    {
                        var medicalRecordDatas = medicalRecord.IPDMedicalRecordDatas;
                        if (medicalRecordDatas != null)
                        {
                            ngayRaVien = medicalRecordDatas.FirstOrDefault(e => e.Code == "IPDMRPTCDRVANS")?.Value;
                        }
                    }
                    break;
                case "Chuyển viện":
                    if (medicalRecord != null)
                    {
                        var medicalRecordDatas = medicalRecord.IPDMedicalRecordDatas;
                        if (medicalRecordDatas != null)
                        {
                            ngayRaVien = medicalRecordDatas.FirstOrDefault(e => e.Code == "IPDMRPTCHVHANS")?.Value;
                        }
                    }
                    break;
                case "Chuyển tuyến":
                    //IPDTD0ANS
                    if (medicalRecord != null)
                    {
                        var medicalRecordDatas = medicalRecord.IPDMedicalRecordDatas;
                        if (medicalRecordDatas != null)
                        {
                            ngayRaVien = medicalRecordDatas.FirstOrDefault(e => e.Code == "IPDTD0ANS")?.Value;
                        }
                    }
                    break;
                case "Tử vong":
                    //
                    if (medicalRecord != null)
                    {
                        var medicalRecordDatas = medicalRecord.IPDMedicalRecordDatas;
                        if (medicalRecordDatas != null)
                        {
                            ngayRaVien = medicalRecordDatas.FirstOrDefault(e => e.Code == "IPDMRPTNGTVANS")?.Value;
                        }
                    }
                    break;
                case "Chuyển khoa":
                    ngayRaVien = Convert.ToDateTime(ipd.DischargeDate).ToString("hh:mm dd/MM/yyyy");
                    break;
            }
            var khoa = ipd.Specialty.ViName;
            Customer customerInfo = ipd.Customer ?? null;
            var translation_util = new TranslationUtil(unitOfWork, ipd.Id, "IPD", "Surgery Certificate");
            var translations = translation_util.GetList();
            return Content(HttpStatusCode.OK, new
            {
                certificate.Id,
                CustomerInfo = customerInfo,
                Khoa = khoa,
                ProcedureDoctor = new { doctor?.Username, doctor?.Fullname, doctor?.DisplayName, doctor?.Title },
                ProcedureTime = certificate.ProcedureTime?.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND),
                Dean = new { dean?.Username, dean?.Fullname, dean?.DisplayName, dean?.Title },
                DeanConfirmTime = certificate.DeanConfirmTime?.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND),
                Director = new { director?.Username, director?.Fullname, director?.DisplayName, director?.Title },
                DirectorTime = certificate.DirectorTime?.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND),
                NgayVaoVien = ngayVaoVien,
                NgayRaVien = ipd.DischargeDate != null ? Convert.ToDateTime(ipd.DischargeDate).ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND) : null,
                IsLocked = IPDIsBlock(ipd, Constant.IPDFormCode.IPDSurgeryCertificate),
                Datas = data,
                CreateAt = certificate.CreatedAt,
                FormId = certificate.FormId?.ToString(),
                Translations = translations
            });
        }

        private TransferInfoModel GetFirstIpd(IPD ipd)
        {
            var spec = ipd.Specialty;
            var current_doctor = ipd.PrimaryDoctor;

            var transfers = new IPDTransfer(ipd).GetListInfo();
            TransferInfoModel first_ipd = null;
            if (transfers.Count() > 0)
            {
                first_ipd = transfers.FirstOrDefault(e => e.CurrentType == "ED" || e.CurrentType == "IPD" && (string.IsNullOrEmpty(e.CurrentSpecialtyCode) || !e.CurrentSpecialtyCode.Contains("PTTT")));
            }
            else
            {
                first_ipd = new TransferInfoModel()
                {
                    CurrentRawDate = ipd.AdmittedDate,
                    CurrentSpecialty = new { spec?.ViName, spec?.EnName },
                    CurrentDoctor = new { current_doctor?.Username, current_doctor?.Fullname, current_doctor?.DisplayName },
                    CurrentDate = ipd.AdmittedDate.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND),
                };
            }
            if (first_ipd == null)
            {
                first_ipd = new TransferInfoModel()
                {
                    CurrentRawDate = ipd.AdmittedDate,
                    CurrentSpecialty = new { spec?.ViName, spec?.EnName },
                    CurrentDoctor = new { current_doctor?.Username, current_doctor?.Fullname, current_doctor?.DisplayName },
                    CurrentDate = ipd.AdmittedDate.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND),
                };
            }
            return first_ipd;
        }      

        protected SurgeryAndProcedureSummaryV3 GetProcedureSummary(Guid? id)
        {
            return unitOfWork.SurgeryAndProcedureSummaryV3Repository.FirstOrDefault(e => !e.IsDeleted && e.Id == id);
        }

        protected List<MappingData> FormatString(List<MappingData> datas)
        {
            int lengthOfdatas = datas.Count;
            for (int i = 0; i < lengthOfdatas; i++)
            {
                if (datas[i].Code == "IPDSURCER08")
                {
                    var stringObject = datas.FirstOrDefault(o => o.Code == "IPDSURCER10");
                    string jsonText = stringObject?.Value;
                    if (jsonText == null || jsonText == $"\"\"")
                        jsonText = "";
                    JavaScriptSerializer jss = new JavaScriptSerializer();
                    List<Dictionary<string, string>> objs = jss.Deserialize<List<Dictionary<string, string>>>(jsonText);
                    string _str = String.Empty;
                    if (objs != null)
                    {
                        int lengthOfobjs = objs.Count;
                        for (int j = 0; j < lengthOfobjs; j++)
                        {
                            var codeIcd10 = objs[j]["code"]?.ToString();
                            if (j == 0)
                                _str += codeIcd10;
                            else
                                _str += $" ,{codeIcd10}";
                        }
                        datas[i].Value = datas[i].Value + $" ({_str})";
                    }
                }
                if (datas[i].Code == "IPDSURCER22")
                {
                    var stringObject = datas.FirstOrDefault(o => o.Code == "IPDSURCER12");
                    string jsonText = stringObject?.Value;
                    if (jsonText == null || jsonText == $"\"\"")
                        jsonText = "";
                    JavaScriptSerializer jss = new JavaScriptSerializer();
                    List<Dictionary<string, string>> objs = jss.Deserialize<List<Dictionary<string, string>>>(jsonText);
                    string _str = String.Empty;
                    if (objs != null)
                    {
                        int lengthOfobjs = objs.Count;
                        for (int j = 0; j < lengthOfobjs; j++)
                        {
                            var codeIcd10 = objs[j]["code"]?.ToString();
                            if (j == 0)
                                _str += codeIcd10;
                            else
                                _str += $" ,{codeIcd10}";
                        }
                        datas[i].Value = datas[i].Value + $" ({_str})";
                    }
                }
            }

            return datas.OrderBy(o => o.Code).ToList();
        }

        protected List<MappingData> AutofillFromProcedure(List<MappingData> datas, SurgeryAndProcedureSummaryV3 procedure, Dictionary<string, string> codes)
        {
            var codeKeys = codes.Keys.ToList();
            var codeValues = codes.Values.ToList();
            var procedureId = procedure?.Id;
            var procedureVisit = procedure?.VisitId;
            var dataProcedure  = unitOfWorkDapper.FormDatasRepository.Find(e =>
                    e.IsDeleted == false &&
                    e.VisitId == procedureVisit &&
                    e.FormId == procedureId
            ).Select(f => new FormDataValue { Id = f.Id, Code = f.Code, Value = f.Value, FormId = f.FormId, FormCode = f.FormCode }).ToList();
        

            foreach (var item in codeKeys)
            {
                var check = datas.FirstOrDefault(e => e.Code == item);
                if (check == null)
                {
                    MappingData _new = new MappingData()
                    {
                        Code = item,
                        Value = null
                    };
                    datas.Add(_new);
                }
            }

            var dataResult = (from d in datas
                              select new MappingData()
                              {
                                  Code = d.Code,
                                  Value = ChangeValue(d, dataProcedure, codes).Value,
                              }).ToList();

            return dataResult;
        }

        protected MappingData ChangeValue(MappingData data, List<FormDataValue> dataProcedure, Dictionary<string, string> codes)
        {
            var codeKeys = codes.Keys.ToList();
            if (codeKeys.Contains(data.Code))
            {
                string key = data.Code;
                data.Value = dataProcedure.FirstOrDefault(e => e.Code == codes[key]) == null ? "" : dataProcedure.FirstOrDefault(e => e.Code == codes[key]).Value;
                //DateTime createdDay = new DateTime(2022, 9, 15);
                //if (data.Value.Count() > 0 && (key == "IPDSURCER18" || key == "IPDSURCER20") && procedure.CreatedAt >= createdDay)
                //{
                //    var obj = new JavaScriptSerializer().Deserialize<dynamic>(data.Value);
                //    string fullname = string.Empty;
                //    foreach (var item in obj)
                //    {
                //        var username = (string)item;
                //        var name = unitOfWork.UserRepository.FirstOrDefault(e => !e.IsDeleted && e.Username == username);
                //        if(name != null)
                //        {
                //            var full_name = name.Fullname;
                //            fullname = fullname + ", " + full_name;
                //        }                        
                //    }
                //    data.Value = fullname;
                //}
            }
            return data;
        }

        protected IPDSurgeryCertificate GetSurgeryCertificateById(Guid id)
        {
            return unitOfWork.IPDSurgeryCertificateRepository.FirstOrDefault(e => !e.IsDeleted && e.Id == id);
        }

        public class MappingData
        {
            public string Code { get; set; }
            public string Value { get; set; }
        }
    }
}