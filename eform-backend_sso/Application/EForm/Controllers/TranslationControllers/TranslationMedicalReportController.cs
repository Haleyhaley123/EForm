using DataAccess.Models;
using DataAccess.Models.EDModel;
using DataAccess.Models.OPDModel;
using DataAccess.Models.IPDModel;
using EForm.Authentication;
using EForm.BaseControllers;
using EForm.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using EForm.Utils;
using DataAccess.Models.EOCModel;
using EForm.Models.IPDModels;
using static EForm.Controllers.IPDControllers.IPDSurgeryCertificateController;
using DataAccess.Models.EIOModel;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace EForm.Controllers.TranslationControllers
{
    [SessionAuthorize]
    public class TranslationMedicalReportController : BaseApiController
    {
        [HttpGet]
        [Route("api/Translation/MedicalReport/Document/{id}")]
        [Permission(Code = "TMERE1")]
        public IHttpActionResult GetTranslationDocumentMedicalReportAPI(Guid id)
        {
            var translation = unitOfWork.TranslationRepository.GetById(id);
            if (translation == null)
                return Content(HttpStatusCode.NotFound, Message.TRANSLATION_NOT_FOUND);

            dynamic visit = null;
            dynamic response = null;
            if (translation.VisitTypeGroupCode.Equals("ED"))
            {
                visit = unitOfWork.EDRepository.GetById((Guid)translation.VisitId);
                response = BuildEDMedicalReport(visit, translation);
            }
            else if (translation.VisitTypeGroupCode.Equals("OPD"))
            {
                visit = unitOfWork.OPDRepository.GetById((Guid)translation.VisitId);
                response = BuildOPDMedicalReport(visit, translation);
            }
            else if (translation.VisitTypeGroupCode.Equals("IPD"))
            {
                visit = unitOfWork.IPDRepository.GetById((Guid)translation.VisitId);
                response = BuildIPDMedicalReport(visit, translation);
            }
            else if (translation.VisitTypeGroupCode.Equals("EOC"))
            {
                visit = unitOfWork.EOCRepository.GetById((Guid)translation.VisitId);
                response = BuildEOCMedicalReport(visit, translation);
            }
            if (visit == null)
                return Content(HttpStatusCode.NotFound, Message.VISIT_NOT_FOUND);

            return Content(HttpStatusCode.OK, response);
        }

        [HttpGet]
        [Route("api/Translation/MedicalReport/Trans/{id}")]
        [Permission(Code = "TMERE2")]
        public IHttpActionResult GetTranslationDetailAPI(Guid id)
        {
            var translation = unitOfWork.TranslationRepository.GetById(id);
            if (translation == null)
                return Content(HttpStatusCode.NotFound, Message.TRANSLATION_NOT_FOUND);

            OPDOutpatientExaminationNote oen = null;
            IPD visit_ipd = null;
            List<dynamic> from_datas = new List<dynamic>();
            var clinic = translation.VisitTypeGroupCode + "_"+ translation.EnName + "_" + translation.FromLanguage;
            if (translation.VisitTypeGroupCode.Equals("ED"))
                from_datas = GetEDMedicalReport((Guid)translation.VisitId, translation.FromLanguage, translation.EnName);
            else if (translation.VisitTypeGroupCode.Equals("OPD"))
                from_datas = GetOPDMedicalReport((Guid)translation.VisitId, translation.FromLanguage, out oen,translation.EnName);
            else if (translation.VisitTypeGroupCode.Equals("IPD"))
                from_datas = GetIPDMedicalReport((Guid)translation.VisitId, translation.FromLanguage, out visit_ipd, translation.EnName);
            else if (translation.VisitTypeGroupCode.Equals("EOC"))
                from_datas = GetEOCMedicalReport((Guid)translation.VisitId, translation.FromLanguage, translation.EnName);

            var to_datas = translation.TranslationDatas.Where(e => !e.IsDeleted).Select(e => new { e.Id, e.Code, e.Value }).ToList();
            if(to_datas.Count == 0)
            {
                var datas = unitOfWork.TranslationRepository.Find(x => x.VisitId == translation.VisitId && x.ToLanguage == translation.ToLanguage).Join(unitOfWork.TranslationDataRepository.AsQueryable(), trans => trans.Id, transData => transData.TranslationId, (trans, transData) => new
                {
                    Id = transData.Id,
                    Code = transData.Code,
                    Value = transData.Value,
                    UpdatedAt = transData.UpdatedAt
                }).OrderByDescending(x => x.UpdatedAt).ToList().Join(unitOfWork.MasterDataRepository.AsQueryable(),tdata => tdata.Code,m => m.Code,(tdata, m) => new
                {
                    Id = tdata.Id,
                    Code = tdata.Code,
                    Value = tdata.Value,
                    UpdatedAt = tdata.UpdatedAt,
                    DefaultValue = m.DefaultValue
                }).ToList();
                to_datas = unitOfWork.MasterDataRepository.Find(x => x.Clinic.Contains(clinic) && x.Level == 2).OrderBy(x => x.CreatedAt).Select(e => new
                {
                   Id = e.Id,
                   Code = e.Code,
                   Value = e.DefaultValue != null ? datas.FirstOrDefault(y => y.DefaultValue == e.DefaultValue)?.Value : datas.FirstOrDefault(y => y.Code == e.Code)?.Value,
                  
                }).ToList();
            }
            else
            {
                to_datas = unitOfWork.MasterDataRepository.Find(x => x.Clinic.Contains(clinic) && x.Level == 2).OrderBy(x => x.CreatedAt).Select(e => new
                {
                    Id = e.Id,
                    Code = e.Code,
                    Value = to_datas.FirstOrDefault(y => y.Code == e.Code)?.Value,

                }).ToList();
            }
            
            var first_ipd = GetFirstIpdInVisitTypeIPD(visit_ipd);
            return Content(HttpStatusCode.OK, new
            {
                translation.Id,
                translation.VisitId,
                translation.VisitTypeGroupCode,
                translation.ViName,
                translation.EnName,
                translation.FromLanguage,
                translation.ToLanguage,
                translation.Note,
                translation.Status,
                FromDatas = from_datas,
                Datas = to_datas,
                translation.CreatedBy,
                PkntVersion = oen?.Version,
                AdmittedDateFirstIpd = first_ipd?.CurrentDate,
                InfoCustomer = new {
                    PID = translation.PID,
                    VisitCode = translation.VisitCode,
                    FullName = translation.CustomerName
                }
            });
        }

        public dynamic BuildEDMedicalReport(ED ed, Translation translation)
        {
            var trans_datas = translation.TranslationDatas.Where(e => !e.IsDeleted);

            var customer = ed.Customer;
            var site = GetSite();

            var etr = ed.EmergencyTriageRecord;

            var emer_record = ed.EmergencyRecord;
            var emer_record_datas = emer_record.EmergencyRecordDatas;

            var discharge_info = ed.DischargeInformation;
            var discharge_info_datas = discharge_info.DischargeInformationDatas;
            var icd = discharge_info_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0DIAICD")?.Value;
            var icd_option = discharge_info_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0DIAOPT")?.Value;
            var DischargeDate = GetDischargeDate(ed);
            return new
            {
                site?.Location,
                Site = site?.Name,
                site?.Province,
                Date = DischargeDate?.ToString(Constant.DATE_FORMAT),
                customer.Fullname,
                DateOfBirth = customer.DateOfBirth?.ToString(Constant.DATE_FORMAT),
                customer.PID,
                AdmittedDate = ed.AdmittedDate.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND),
                DischargeDate = DischargeDate?.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND),
                ChiefComplain = etr?.EmergencyTriageRecordDatas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "ETRCC0ANS")?.Value,
                History = emer_record_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "ER0HISANS")?.Value,
                Assessment = new EmergencyRecordAssessment(emer_record.Id).GetList(),
                ResultOfParaclinicalTests = discharge_info_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0RPTANS2")?.Value,
                Diagnosis = discharge_info_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0DIAANS")?.Value,
                ICD = icd,
                ICDOption = icd_option,
                TreatmentAndProcedures = discharge_info_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0TAPANS")?.Value,
                SignificantMedications = discharge_info_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0SM0ANS2")?.Value,
                CurrentStatus = discharge_info_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0CS0ANS")?.Value,
                FollowupCarePlan = discharge_info_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0FCPANS")?.Value,
                DoctorRecommendations = discharge_info_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0DR0ANS")?.Value,
                Translation = new
                {
                    site?.Location,
                    Site = site?.Name,
                    site?.Province,
                    Date = DischargeDate?.ToString(Constant.DATE_FORMAT),
                    customer.Fullname,
                    DateOfBirth = customer.DateOfBirth?.ToString(Constant.DATE_FORMAT),
                    customer.PID,
                    AdmittedDate = ed?.AdmittedDate.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND),
                    DischargeDate = DischargeDate?.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND),
                    ChiefComplain = trans_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "TRANSLATEREASONANS")?.Value,
                    History = trans_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "TRANSLATEHISTORYOFPRESENTANS")?.Value,
                    Assessment = trans_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "TRANSLATEASSESSMENTANS")?.Value,
                    ResultOfParaclinicalTests = trans_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "TRANSLATESUBRESULTANS")?.Value,
                    Diagnosis = trans_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "TRANSLATEPDIAGNOSISANS")?.Value,
                    ICD = icd,
                    ICDOption = icd_option,
                    TreatmentAndProcedures = trans_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "TRANSLATETREATMENTANDPROCEDUREANS")?.Value,
                    SignificantMedications = trans_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "TRANSLATEDRUGUSEDANS")?.Value,
                    CurrentStatus = trans_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "TRANSLATECURSTATUSANS")?.Value,
                    FollowupCarePlan = trans_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "TRANSLATECAREPLANANS")?.Value,
                    DoctorRecommendations = trans_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "TRANSLATERECOMMENDATIONANDFOLLOWUPANS")?.Value,
                },
            };
        }
        public dynamic BuildOPDMedicalReport(OPD opd, Translation translation)
        {
            var trans_datas = translation.TranslationDatas.Where(e => !e.IsDeleted);

            var customer = opd.Customer;
            var dob = customer.DateOfBirth?.ToString(Constant.DATE_FORMAT);
            var gender = new CustomerUtil(customer).GetGender();
            var clinic = opd.Clinic;

            var oen = opd.OPDOutpatientExaminationNote;
            var date_of_visit = opd.AdmittedDate.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND);

            var oen_datas = oen.OPDOutpatientExaminationNoteDatas.Where(e => !e.IsDeleted).ToList();
            var icd_diagnosis = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENICDANS")?.Value;
            var icd_option = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENICDOPT")?.Value;
            var date_of_next_appointment = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENDORANS")?.Value;

            var physician = opd.PrimaryDoctor;
            if (opd.AuthorizedDoctorId != null)
                physician = null;
            var physician_name = physician?.Fullname;

            var date_now = DateTime.Now.ToString(Constant.DATE_FORMAT);

            string clinicCode = ClinicalExaminationAndFindings.GetStringClinicCodeUsed(opd);

            var site = GetSite();
            return new
            {
                site?.Location,
                Site = site?.Name,
                site?.Province,
                Date = date_now,
                customer.PID,
                customer.Fullname,
                ClinicCode = clinic.Code,
                Gender = gender,
                DateOfBirth = dob,
                customer.Address,
                opd.IsTelehealth,
                DateOfVisit = date_of_visit,
                ReasonOfVisit = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENCC0ANS")?.Value,
                HistoryOfPresentIllness = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENHPIANS")?.Value,
                ClinicalExaminationAndFindings = new ClinicalExaminationAndFindings(clinicCode, clinic?.Data, oen.Id, opd.IsTelehealth, oen.Version).GetData(),
                PrincipalTest = getOPDPrincipalTest(oen),
                Diagnosis = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENDD0ANS")?.Value,
                ICDDiagnosis = icd_diagnosis,
                ICDOption = icd_option,
                TreatmentPlans = getOPDTreatmentPlans(oen),
                RecommendtionAndFollowUp = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENRFUANS")?.Value,
                DateOfNextAppointment = date_of_next_appointment,
                Physician = physician_name,
                Copy = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENCOPYANS")?.Value,

                Translation = new
                {
                    site?.Location,
                    Site = site?.Name,
                    site?.Province,
                    Date = date_now,
                    customer.PID,
                    customer.Fullname,
                    Gender = trans_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "TRANSLATEGENANS")?.Value,
                    DateOfBirth = dob,
                    Address = trans_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "TRANSLATEADDANS")?.Value,
                    opd.IsTelehealth,
                    DischargeDate = opd.DischargeDate?.ToString(Constant.DATE_FORMAT),
                    DateOfVisit = date_of_visit,
                    ReasonOfVisit = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATEREASONFORVISITANS")?.Value,
                    HistoryOfPresentIllness = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATEHISTORYOFPRESENTANS")?.Value,
                    ClinicalExaminationAndFindings = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATEKLSANS")?.Value,
                    PrincipalTest = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATEPRINCIPALTESTANS")?.Value,
                    Diagnosis = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATEPDIAGNOSISANS")?.Value,
                    ICDDiagnosis = icd_diagnosis,
                    ICDOption = icd_option,
                    TreatmentPlans = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATETREANTMENTPLANANS")?.Value,
                    RecommendtionAndFollowUp = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATERECOMMENDATIONANDFOLLOWUPANS")?.Value,
                    DateOfNextAppointment = date_of_next_appointment,
                    Physician = physician_name,
                    PkntVersion = oen.Version
                }
            };
        }
        protected TransferInfoModel GetFirstIpd(IPD ipd)
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
        public dynamic BuildIPDMedicalReport(IPD ipd, Translation translation)
        {
            var trans_datas = translation.TranslationDatas.Where(e => !e.IsDeleted);

            var customer = ipd.Customer;
            List<string> address = new List<string>();
            if (!string.IsNullOrEmpty(customer?.MOHAddress))
                address.Add(customer?.MOHAddress);
            if (!string.IsNullOrEmpty(customer?.MOHDistrict))
                address.Add(customer?.MOHDistrict);
            if (!string.IsNullOrEmpty(customer?.MOHProvince))
                address.Add(customer?.MOHProvince);

            var medical_record = ipd.IPDMedicalRecord;
            var status = ipd.EDStatus;
            var first_ipd = GetFirstIpd(ipd);
            // var admission = ipd.AdmittedDate.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND);
            var admission = first_ipd.CurrentDate;

            var discharge = ipd.DischargeDate?.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND);

            var part_2 = medical_record?.IPDMedicalRecordPart2;
            var part_2_datas = part_2?.IPDMedicalRecordPart2Datas.Where(e => !e.IsDeleted).ToList();

            var part_3 = medical_record?.IPDMedicalRecordPart3;
            var part_3_datas = part_3?.IPDMedicalRecordPart3Datas.Where(e => !e.IsDeleted).ToList();
            var clinical_evolution = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPEQTBLANS")?.Value;
            var result_of_paraclinical_tests = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPETTKQANS")?.Value;
            var diagnosis = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPECDBCANS")?.Value;
            var icd_diagnosis = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPEICDCANS")?.Value;
            var co_morbidities = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPECDKTANS")?.Value;
            var icd_co_morbidities = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPEICDPANS")?.Value;
            var treatments_and_procedures = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPEPPDTANS")?.Value;
            var significant_medications = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPETCDDANS")?.Value;
            var condition_at_discharge = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPETTNBANS")?.Value;
            var followup_care_plan = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPEHDTVANS")?.Value;

            var discharge_mr = ipd?.IPDDischargeMedicalReport;

            var physician_in_charge = discharge_mr?.PhysicianInCharge;
            var physician_in_charge_info = new { physician_in_charge?.Username, physician_in_charge?.Fullname, physician_in_charge?.Title, physician_in_charge?.DisplayName };
            var physician_in_charge_time = discharge_mr?.PhysicianInChargeTime?.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND);


            var dept_head = discharge_mr?.DeptHead;
            var dept_head_info = new { dept_head?.Username, dept_head?.Fullname, dept_head?.Title, dept_head?.DisplayName };
            var dept_head_time = discharge_mr?.DeptHeadTime?.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND);


            var director = discharge_mr?.Director;
            var director_info = new { director?.Username, director?.Fullname, director?.Title, director?.DisplayName };
            var director_time = discharge_mr?.DirectorTime?.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND);
            var firstipd = GetFirstIpdInVisitTypeIPD(ipd);
            var site = GetSite();
            //Chẩn đoán bệnh chính
            string reportDiagnosis = "";
            var chanDoanBenhChinh = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPECDBCANS")?.Value;
            string maChanDoanBenhChinh = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPEICDCANS")?.Value;
            List<DiagnosisModel> mainDiagnosisCodes = new List<DiagnosisModel>();
            string mainCodes = "";
            if (maChanDoanBenhChinh != null)
            {
                mainDiagnosisCodes = JsonConvert.DeserializeObject<List<DiagnosisModel>>(maChanDoanBenhChinh);
                if (mainDiagnosisCodes != null)
                {
                    mainCodes = mainDiagnosisCodes[0].code;
                }
            }

            //Chẩn đoán bệnh kèm theo
            var chanDoanBenhKemTheo = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPECDKTANS")?.Value;
            string maChanDoanBenhKemTheo = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPEICDPANS")?.Value;
            string optionCodes = "";
            if (maChanDoanBenhKemTheo != null)
            {
                List<DiagnosisModel> optionDiagnosisCodes = JsonConvert.DeserializeObject<List<DiagnosisModel>>(maChanDoanBenhKemTheo);
                if (optionDiagnosisCodes != null)
                {
                    optionCodes = String.Join(", ", optionDiagnosisCodes.Select(e => e.code).ToArray());
                }
            }

            if (!string.IsNullOrEmpty(chanDoanBenhChinh) && !string.IsNullOrEmpty(chanDoanBenhKemTheo))
            {
                chanDoanBenhChinh += ", ";
            }
            if (!string.IsNullOrEmpty(mainCodes) && !string.IsNullOrEmpty(optionCodes))
            {
                mainCodes += ", ";
            }

            reportDiagnosis = $"{chanDoanBenhChinh}{chanDoanBenhKemTheo} ({mainCodes}{optionCodes})";

            if (string.IsNullOrEmpty(mainCodes) && string.IsNullOrEmpty(optionCodes))
            {
                reportDiagnosis.Replace(" ()", "");
            }
            var medical_record_datas = medical_record.IPDMedicalRecordDatas.Where(e => !e.IsDeleted).ToList();
            string reason_for_transfer = medical_record_datas.FirstOrDefault(e => !e.IsDeleted && e.Code == "IPDRFTANS")?.Value;
            var time_of_transfer = medical_record_datas.FirstOrDefault(e => !e.IsDeleted && e.Code == "IPDMRPTCHVHANS")?.Value;
            var receiving_person = medical_record_datas.FirstOrDefault(e => !e.IsDeleted && e.Code == "IPDNRHANS")?.Value;
            var transportation_method = medical_record_datas.FirstOrDefault(e => !e.IsDeleted && e.Code == "IPDTM0ANS")?.Value;
            var escort_person = medical_record_datas.FirstOrDefault(e => !e.IsDeleted && e.Code == "IPDNATANS")?.Value;
            string followupCarePlanTrans = "";
            string noteTrans = "";
            string resultOfParaclinicalTestsTrans = "";
            string treatmentsAndProceduresTrans = "";
            string conditionAtDischargeTrans = "";
            string chiefComplaintTrans = "";
            string currentStatusTrans = "";
            string transportationMethodTrans = "";
            string addressTrans = "";
            string escortPersontrans = "";
            switch (translation.EnName.ToUpper())
            {
                case "MEDICAL REPORT":
                    followupCarePlanTrans = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATECAREPLANANS")?.Value;
                    treatmentsAndProceduresTrans = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATETREATMENTANDPROCEDUREANS")?.Value;
                    chiefComplaintTrans = trans_datas.FirstOrDefault(e => e.Code == "TRANSLATEREASONANS")?.Value;
                    conditionAtDischargeTrans = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATCURRENTPATIENTANS")?.Value;
                    resultOfParaclinicalTestsTrans = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATESUBRESULTANS")?.Value;
                    currentStatusTrans = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATCURRENTPATIENTANS")?.Value;

                    break;
                case "DISCHARGE MEDICAL REPORT":
                    followupCarePlanTrans = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATEFOLLOWUPCAREPLANANS")?.Value;
                    treatmentsAndProceduresTrans = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATETREATMENTANDPROCEDUREANS")?.Value;
                    chiefComplaintTrans = trans_datas.FirstOrDefault(e => e.Code == "TRANSLATEREASONANS")?.Value;
                    addressTrans = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATEADDANS")?.Value;
                    resultOfParaclinicalTestsTrans = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATECONDITIONDISCHARGEANS")?.Value;
                    break;
                case "REFERRAL LETTER":
                    followupCarePlanTrans = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATEFOLLOWUPCAREPLANANS")?.Value;
                    resultOfParaclinicalTestsTrans = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATESUBRESULTANS")?.Value;
                    treatmentsAndProceduresTrans = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATETREATMENTANDPROCEDUREANS")?.Value;
                    conditionAtDischargeTrans = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATESTATUSPATIENTTRANSFERANS")?.Value;
                    chiefComplaintTrans = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATECHIEFCOMANS")?.Value;
                    transportationMethodTrans = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATETRANSPORTANS")?.Value;
                    addressTrans = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATEADDFOOTERANS")?.Value;
                    escortPersontrans = trans_datas.FirstOrDefault(e => e.Code == "TRANSLATESENDERANS")?.Value;
                    break;
                case "DISCHARGE CERTIFICATE":
                    noteTrans = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATENOTEANS")?.Value;
                    treatmentsAndProceduresTrans = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATETREATMENTANDPROCEDUREANS")?.Value;
                    conditionAtDischargeTrans = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATECONDITIONDISCHARGEANS")?.Value;
                    addressTrans = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATEADDANS")?.Value;
                    break;
                case "TRANSFER LETTER":
                    resultOfParaclinicalTestsTrans = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATELABANDSUBRESULTANS")?.Value;
                    treatmentsAndProceduresTrans = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATEMPTDRUGUSETREATMENTANS")?.Value;
                    conditionAtDischargeTrans = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATESATATUSREFERALANS")?.Value;
                    followupCarePlanTrans = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATETREANTMENTPLANANS")?.Value;
                    transportationMethodTrans = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATETRANSPORTFOOTERANS")?.Value;
                    escortPersontrans = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATEPERTRANSPORTANS")?.Value;
                    break;
                case "INJURY CERTIFICATE":
                    treatmentsAndProceduresTrans = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATETREATMENTANS")?.Value;
                    chiefComplaintTrans = trans_datas.FirstOrDefault(e => e.Code == "TRANSLATEREASONANS")?.Value;
                    break;
                case "SURGERY CERTIFICATE":
                    addressTrans = trans_datas?.FirstOrDefault(e => e.Code == "TRANSLATEADDANS")?.Value;
                    break;
            }
            return new
            {
                site?.Location,
                Site = site?.Name,
                site?.Province,
                Specialty = ipd.Specialty?.ViName,
                customer.PID,
                customer.Fullname,
                Gender = new CustomerUtil(customer).GetGender(),
                DateOfBirth = customer.DateOfBirth?.ToString(Constant.DATE_FORMAT),
                Address = string.Join(", ", address),
                customer.Nationality,
                customer.AgeFormated,
                Ethnic = customer.MOHEthnic,
                ipd.HealthInsuranceNumber,
                ExpireHealthInsuranceDate = ipd.ExpireHealthInsuranceDate?.ToString(Constant.DATE_FORMAT),
                Admission = admission,
                Discharge = discharge,
                ChiefComplaint = part_2_datas?.FirstOrDefault(e => e.Code == "IPDMRPTLDVVANS")?.Value,
                ClinicalEvolution = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPEQTBLANS")?.Value,
                ResultOfParaclinicalTests = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPETTKQANS")?.Value,
                Diagnosis = reportDiagnosis,
                ICDDiagnosis = icd_diagnosis,
                CoMorbidities = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPECDKTANS")?.Value,
                ICDCoMorbidities = icd_co_morbidities,
                TreatmentsAndProcedures = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPEPPDTANS")?.Value,
                SignificantMedications = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPETCDDANS")?.Value,
                ConditionAtDischarge = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPETTNBANS")?.Value,
                FollowUpCarePlan = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPEHDTVANS")?.Value,
                Date = discharge,
                PhysicianInCharge = physician_in_charge_info,
                PhysicianInChargeTime = physician_in_charge_time,
                DeptHead = dept_head_info,
                DeptHeadTime = dept_head_time,
                Director = director_info,
                DirectorTime = director_time,
                DoctorRecommendations = part_3_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "IPDMRPEHDTVANS")?.Value,
                DrugsUsed = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPETCDDANS")?.Value,
                HospitalizedStatus = part_2_datas?.FirstOrDefault(e => e.Code == "IPDMRPTTTBAANS")?.Value,
                DischargeStatus = part_3_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "IPDMRPETTNBANS")?.Value,
                ReasonForTransfer = reason_for_transfer,
                TimeOfTransfer = time_of_transfer,
                ReceivingPerson = receiving_person,
                TransportationMethod = transportation_method,
                EscortPerson = escort_person,
                customer.MOHJob,
                customer.WorkPlace,
                IdCard = customer.IdentificationCard,
                customer.IssueDate,
                customer.IssuePlace,
                PreoperativeDiagnosisTrans = trans_datas.FirstOrDefault(e => e.Code == "TRANSLATEPREDIAGNOSISANS")?.Value,
                PostoperativeDiagnosisTrans = trans_datas.FirstOrDefault(e => e.Code == "TRANSLATEDIAGNOSISAFTERSERGERYANS")?.Value,
                ProcedurePerformedTrans = trans_datas.FirstOrDefault(e => e.Code == "TRANSLATEPPPTANS")?.Value,
                MethodOfAnesthesiaTrans = trans_datas.FirstOrDefault(e => e.Code == "TRANSLATEPPVC")?.Value,
                Translation = new
                {
                    site?.Location,
                    Site = site?.Name,
                    site?.Province,
                    Specialty = ipd.Specialty?.ViName,
                    customer.PID,
                    customer.Fullname,
                    Gender = trans_datas.FirstOrDefault(e => !e.IsDeleted && e.Code == "TRANSLATEGENANS")?.Value,
                    DateOfBirth = customer.DateOfBirth?.ToString(Constant.DATE_FORMAT),
                    Nationality = trans_datas.FirstOrDefault(e => !e.IsDeleted && e.Code == "TRANSLATENATANS")?.Value,
                    Address = trans_datas.FirstOrDefault(e => !e.IsDeleted && e.Code == "TRANSLATEADDANS")?.Value,
                    DateOfVisit = ipd.AdmittedDate.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND),
                    Admission = admission,
                    Discharge = discharge,
                    ChiefComplaint = chiefComplaintTrans,
                    ClinicalEvolution = trans_datas.FirstOrDefault(e => e.Code == "TRANSLATECLINEVOLUANS")?.Value,
                    ResultOfParaclinicalTests = resultOfParaclinicalTestsTrans,
                    Diagnosis = trans_datas.FirstOrDefault(e => e.Code == "TRANSLATEPDIAGNOSISANS")?.Value,
                    ICDDiagnosis = icd_diagnosis,
                    CoMorbidities = trans_datas.FirstOrDefault(e => e.Code == "TRANSLATECOMORBIDITIESANS")?.Value,
                    ICDCoMorbidities = icd_co_morbidities,
                    TreatmentsAndProcedures = treatmentsAndProceduresTrans,
                    SignificantMedications = trans_datas.FirstOrDefault(e => e.Code == "TRANSLATEDRUGUSEDANS")?.Value,
                    ConditionAtDischarge = conditionAtDischargeTrans,
                    FollowUpCarePlan = followupCarePlanTrans,
                    Date = discharge,
                    PhysicianInCharge = physician_in_charge_info,
                    PhysicianInChargeTime = physician_in_charge_time,
                    DeptHead = dept_head_info,
                    DeptHeadTime = dept_head_time,
                    Director = director_info,
                    DirectorTime = director_time,
                    CurrentStatus = trans_datas.FirstOrDefault(e => e.Code == "TRANSLATECURSTATUSANS")?.Value,
                    DoctorRecommendations = noteTrans,
                    DrugsUsed = trans_datas.FirstOrDefault(e => e.Code == "TRANSLATEDRUGUSEDANS")?.Value,
                    HospitalizedStatus = trans_datas.FirstOrDefault(e => e.Code == "TRANSLATESTATUSADMITTEDANS")?.Value,
                    DischargeStatus = currentStatusTrans,
                    MOHJob = trans_datas.FirstOrDefault(e => e.Code == "TRANSLATEJOBANS")?.Value,
                    ReasonForTransfer = trans_datas.FirstOrDefault(e => e.Code == "TRANSLATEREASONTRANSFERANS")?.Value,
                    ReceivingPerson = trans_datas.FirstOrDefault(e => e.Code == "TRANSLATENAMEMETHODCONTACTEDANS")?.Value,
                    TransportationMethod = transportationMethodTrans,
                    EscortPerson = escortPersontrans,
                    WorkPlace = trans_datas.FirstOrDefault(e => e.Code == "TRANSLATEWORKPLACEANS")?.Value,
                },
                AdmittedDateFirstIpd = firstipd.CurrentDate
            };
        }
        public dynamic BuildEOCMedicalReport(EOC opd, Translation translation)
        {
            var trans_datas = translation.TranslationDatas.Where(e => !e.IsDeleted);

            var customer = opd.Customer;
            var dob = customer.DateOfBirth?.ToString(Constant.DATE_FORMAT);
            var gender = new CustomerUtil(customer).GetGender();

            var oen = GetOutpatientExaminationNote(opd.Id);
            var date_of_visit = opd.AdmittedDate.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND);

            var oen_datas = GetFormData(opd.Id, oen.Id, "OPDOEN");
            var icd_diagnosis = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENICDANS")?.Value;
            var icd_option = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENICDOPT")?.Value;
            var date_of_next_appointment = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENDORANS")?.Value;

            var physician = opd.PrimaryDoctor;

            var physician_name = physician?.Fullname;

            var date_now = DateTime.Now.ToString(Constant.DATE_FORMAT);

            var site = GetSite();
            return new
            {
                site?.Location,
                Site = site?.Name,
                site?.Province,
                Date = date_now,
                customer.PID,
                customer.Fullname,
                ClinicCode = "Normal",
                Gender = gender,
                DateOfBirth = dob,
                customer.Address,
                IsEoc = true,
                DateOfVisit = date_of_visit,
                ReasonOfVisit = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENCC0ANS")?.Value,
                HistoryOfPresentIllness = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENHPIANS")?.Value,
                ClinicalExaminationAndFindings = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENCEFANS")?.Value,
                PrincipalTest = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENPT0ANS")?.Value,
                Diagnosis = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENDD0ANS")?.Value,
                ICDDiagnosis = icd_diagnosis,
                ICDOption = icd_option,
                TreatmentPlans = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENTP0ANS")?.Value,
                RecommendtionAndFollowUp = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENRFUANS")?.Value,
                DateOfNextAppointment = date_of_next_appointment,
                Physician = physician_name,
                Copy = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENCOPYANS")?.Value,

                Translation = new
                {
                    site?.Location,
                    Site = site?.Name,
                    site?.Province,
                    Date = date_now,
                    customer.PID,
                    customer.Fullname,
                    Gender = trans_datas.FirstOrDefault(e => !e.IsDeleted && e.Code == "TRANSLATEGENANS")?.Value,
                    DateOfBirth = dob,
                    Address = trans_datas.FirstOrDefault(e => !e.IsDeleted && e.Code == "TRANSLATEADDANS")?.Value,
                    IsEoc = true,
                    DateOfVisit = date_of_visit,
                    ReasonOfVisit = trans_datas.FirstOrDefault(e => e.Code == "TRANSLATEREASONFORVISITANS")?.Value,
                    HistoryOfPresentIllness = trans_datas.FirstOrDefault(e => e.Code == "TRANSLATEHISTORYOFPRESENTANS")?.Value,
                    ClinicalExaminationAndFindings = trans_datas.FirstOrDefault(e => e.Code == "TRANSLATEKLSANS")?.Value,
                    PrincipalTest = trans_datas.FirstOrDefault(e => e.Code == "TRANSLATEPRINCIPALTESTANS")?.Value,
                    Diagnosis = trans_datas.FirstOrDefault(e => e.Code == "TRANSLATEPDIAGNOSIS")?.Value,
                    ICDDiagnosis = icd_diagnosis,
                    ICDOption = icd_option,
                    TreatmentPlans = trans_datas.FirstOrDefault(e => e.Code == "TRANSLATETREANTMENTPLAN")?.Value,
                    RecommendtionAndFollowUp = trans_datas.FirstOrDefault(e => e.Code == "TRANSLATERECOMMENDATIONANDFOLLOWUPANS")?.Value,
                    DateOfNextAppointment = date_of_next_appointment,
                    Physician = physician_name,
                }
            };
        }
        private dynamic GetEDMedicalReport(Guid visit_id, string language, string type)
        {
            var ed = unitOfWork.EDRepository.GetById(visit_id);
            if (ed == null)
                return Content(HttpStatusCode.NotFound, Message.ED_NOT_FOUND);

            List<dynamic> response = new List<dynamic>();

            var etr = ed.EmergencyTriageRecord;
            var chief_complain = etr.EmergencyTriageRecordDatas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "ETRCC0ANS")?.Value;
            response.Add(new { Code = "EDTRANSCC0ANS", Value = chief_complain });

            var emer_record = ed.EmergencyRecord;
            var emer_record_datas = emer_record.EmergencyRecordDatas;
            var history = emer_record_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "ER0HISANS")?.Value;
            var assessment = new EmergencyRecordAssessment(emer_record.Id).GetList();
            response.Add(new { Code = "EDTRANSHISANS", Value = history });
            response.Add(new { Code = "EDTRANSASSANS", Value = assessment });

            var discharge_info = ed.DischargeInformation;
            var discharge_info_datas = discharge_info.DischargeInformationDatas;
            var rop_tests = discharge_info_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0RPTANS2")?.Value;
            var diagnosis = discharge_info_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0DIAANS")?.Value;
            var treatment_procedures = discharge_info_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0TAPANS")?.Value;
            var current_status = discharge_info_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0CS0ANS")?.Value;
            var followup_care_plan = discharge_info_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0FCPANS")?.Value;
            var doctor_recommendations = discharge_info_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0DR0ANS")?.Value;
            var significan_medications = discharge_info_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0SM0ANS2")?.Value;
            var customer = ed.Customer;
            var job = customer.Job;
            var gender = new CustomerUtil(customer).GetGender();
            var address = customer.Address;
            var principal_tests = discharge_info_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0RPTANS2")?.Value;
            var methods = discharge_info_datas?.FirstOrDefault(e => e.Code == "DI0TAPANS")?.Value;
            var patient_status = discharge_info_datas?.FirstOrDefault(e => e.Code == "DI0CS0ANS")?.Value;
            var transportation_method = discharge_info_datas.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0TM0ANS")?.Value;
            response.Add(new { Code = "TRANSLATERESULTPRATESTANS", Value = rop_tests });
            response.Add(new { Code = "TRANSLATEPDIAGNOSISANS", Value = diagnosis });
            response.Add(new { Code = "TRANSLATETREATMENTANDPROCEDUREANS", Value = treatment_procedures });
            response.Add(new { Code = "TRANSLATEDOCTORRECOMENDATIONANS", Value = doctor_recommendations });
            response.Add(new { Code = "TRANSLATEDRUGUSEDANS", Value = significan_medications });
            response.Add(new { Code = "TRANSLATEREASONANS", Value = chief_complain });
            response.Add(new { Code = "TRANSLATECURSTATUSANS", Value = current_status });
            response.Add(new { Code = "TRANSLATECAREPLANANS", Value = followup_care_plan });
            response.Add(new { Code = "TRANSLATEGENANS", Value = gender });
            response.Add(new { Code = "TRANSLATENATANS", Value = customer.Nationality });
            response.Add(new { Code = "TRANSLATEJOBANS", Value = job });
            response.Add(new { Code = "TRANSLATEADDANS", Value = address });
            response.Add(new { Code = "TRANSLATENOTEANS", Value = doctor_recommendations });
            response.Add(new { Code = "TRANSLATESTATUSPATIENTTRANSFER1ANS", Value = current_status });
            response.Add(new { Code = "TRANSLATELABANDSUBRESULTANS", Value = principal_tests });
            response.Add(new { Code = "TRANSLATEMPTDRUGUSETREATMENTANS", Value = methods });
            response.Add(new { Code = "TRANSLATESATATUSREFERALANS", Value = patient_status });
            response.Add(new { Code = "TRANSLATETREANTMENTPLANANS", Value = principal_tests });
            response.Add(new { Code = "TRANSLATEWORKPLACEANS", Value = customer.WorkPlace });
            response.Add(new { Code = "TRANSLATETREATMENTANS", Value = treatment_procedures });
            response.Add(new { Code = "TRANSLATESTATUSINJURYDISCHARGEANS", Value = current_status });
            response.Add(new { Code = "TRANSLATESUBRESULTANS", Value = rop_tests });
            response.Add(new { Code = "TRANSLATEASSESSMENTANS", Value = "" });
            response.Add(new { Code = "TRANSLATEPERTRANSPORTANS", Value = transportation_method });
            response.Add(new { Code = "TRANSLATEDIAGNOSISBEFOREMERANS", Value = diagnosis });
            // response.Add(new { Code = "TRANSLATEREASON", Value = chief_complain });// Quá trình bệnh lý và diễn biến lâm sàng
            var data = (
          from md in unitOfWork.MasterDataRepository.Find(x => x.Clinic.Contains("ED_" + type + "_" + language.ToUpper()) && !x.IsDeleted).Select(md => new {
              md.Code,
              md.Group,
              md.Order,
              md.DataType,
              md.Level,
              md.IsReadOnly,
              md.Note,
              md.Data,
              md.Clinic,
              md.DefaultValue,
              Value = "",
              Id = "",
              md.CreatedAt
          })
           .OrderBy(md => md.CreatedAt)
           .ToList()
          join res in response on md.Code equals res.Code
          select new { md.Code, res.Value }).ToList<dynamic>();
            return data;
        }
        private dynamic GetOPDMedicalReport(Guid visit_id, string language, out OPDOutpatientExaminationNote oen,string type)
        {
            oen = null;
            var opd = unitOfWork.OPDRepository.GetById(visit_id);
            if (opd == null)
                return Content(HttpStatusCode.NotFound, Message.ED_NOT_FOUND);

            List<dynamic> response = new List<dynamic>();

            var customer = opd.Customer;
            var gender = ConvertGender((int)customer.Gender, language);
            var address = customer.Address;
            var nationality = customer.Nationality;
            response.Add(new { Code = "TRANSLATENATANS", Value = nationality });
            response.Add(new { Code = "TRANSLATEGENANS", Value = gender });
            response.Add(new { Code = "TRANSLATEADDANS", Value = address });

            var clinic = opd.Clinic;
            oen = opd.OPDOutpatientExaminationNote;
            var oen_datas = oen?.OPDOutpatientExaminationNoteDatas.Where(e => !e.IsDeleted).ToList();
            var reason_for_visit = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENCC0ANS")?.Value;
            var hisory_of_present_illness = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENHPIANS")?.Value;

            string clinicCode = ClinicalExaminationAndFindings.GetStringClinicCodeUsed(opd);
            var principal_tests = getOPDPrincipalTest(oen);
            var diagnosis = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENDD0ANS")?.Value;
            var treatment_plans = getOPDTreatmentPlans(oen);
            var recommendation_and_follow_up = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENRFUANS")?.Value;
            var reasons_for_admission = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENCC0ANS")?.Value;
            var result_of_paraclinical_tests = getOPDPrincipalTest(oen);
            var treatment_and_procedures = getOPDTreatmentPlans(oen);
            var drugs_used = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENDU0ANS")?.Value;
            var plan_of_care = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENRFUANS")?.Value;
            var patient_status_at_transfer = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENPSAANS")?.Value;
            var job = customer.Job;
            var workPlace = customer.WorkPlace;
            var methods = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENMTUANS")?.Value;
            var transportation_method = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENTM0ANS")?.Value;
            var patient_status = oen_datas.FirstOrDefault(e => e.Code == "OPDOENPS0ANS")?.Value;
            var escort = oen_datas.FirstOrDefault(e => e.Code == "OPDOENNTMANS")?.Value;
            var history = oen_datas.FirstOrDefault(e => e.Code == "OPDOENHPIANS")?.Value;

            var kls = "";
            var clinical_examination_and_findings = hsClinicalExamination(oen) ? new ClinicalExaminationAndFindings(clinicCode, clinic?.Data, oen.Id, opd.IsTelehealth, oen.Version).GetData() : null;
            if(clinical_examination_and_findings != null && clinical_examination_and_findings.Count > 0)
            {
                foreach(var clin  in clinical_examination_and_findings)
                {
                    kls += $"+ {clin.ViName}/ {clin.EnName}: {clin.Value} \n";
                }    
                kls = kls.Substring(0, kls.LastIndexOf("\n"));
            }
            var reason_1 = oen_datas.FirstOrDefault(e => e.Code == "OPDOENRFT1SHT");
            var reason_1_data = unitOfWork.MasterDataRepository.FirstOrDefault(m => m.Code == "OPDOENRFT1SHT");
            var reason_2 = oen_datas.FirstOrDefault(e => e.Code == "OPDOENRFT1LOG");
            var reason_2_data = unitOfWork.MasterDataRepository.FirstOrDefault(m => m.Code == "OPDOENRFT1LOG");
            var reason_for_transfer = new List<dynamic>() {
                new { reason_1_data?.ViName, reason_1?.Value},
                new { reason_2_data?.ViName, reason_2?.Value},
            };
            var reason_for_transfer1 = oen_datas.FirstOrDefault(e => e.Code == "OPDOENRFT3ANS")?.Value;
            var contacted_staff = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENSBCANS")?.Value;
            var medical_escort = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENME0ANS")?.Value;
            response.Add(new { Code = "TRANSLATEREASONFORVISITANS", Value = reason_for_visit });
            response.Add(new { Code = "TRANSLATEHISTORYOFPRESENTANS", Value = hisory_of_present_illness });
            response.Add(new { Code = "TRANSLATECLINTEXAMANS", Value = clinical_examination_and_findings });
            response.Add(new { Code = "TRANSLATEPRINTESTANS", Value = principal_tests });
            response.Add(new { Code = "TRANSLATEPDIAGNOSISANS", Value = diagnosis });
            response.Add(new { Code = "TRANSLATETREANTMENTPLANANS", Value = treatment_plans });
            response.Add(new { Code = "TRANSLATERECOMMENDANS", Value = recommendation_and_follow_up });
            response.Add(new { Code = "TRANSLATECHIEFCOMANS", Value = reasons_for_admission });
            response.Add(new { Code = "TRANSLATERESULTPRATESTANS", Value = result_of_paraclinical_tests });
            response.Add(new { Code = "TRANSLATETREATMENTANDPROCEDUREANS", Value = oen?.IsConsultation == true && !string.IsNullOrEmpty(treatment_and_procedures) ? string.Format("{0}{1}", "\n", treatment_and_procedures) : treatment_and_procedures, });
            response.Add(new { Code = "TRANSLATEDRUGUSEDANS", Value = drugs_used });
            response.Add(new { Code = "TRANSLATECAREPLANANS", Value = plan_of_care });
            response.Add(new { Code = "TRANSLATECURRENTPATIENTANS", Value = patient_status_at_transfer });
            response.Add(new { Code = "TRANSLATEJOBANS", Value = job });
            response.Add(new { Code = "TRANSLATEWORKPLACEANS", Value = workPlace });
            response.Add(new { Code = "TRANSLATECURRENTSATATUSANS", Value = patient_status });
            response.Add(new { Code = "TRANSLATETREATANDDRUGANS", Value = methods });
            response.Add(new { Code = "TRANSLATETRANSPORTANS", Value = transportation_method });
            response.Add(new { Code = "TRANSLATESATATUSREFERALANS", Value = patient_status });
            response.Add(new { Code = "TRANSLATEMPTDRUGUSETREATMENTANS", Value = methods });
            response.Add(new { Code = "TRANSLATELABANDSUBRESULTANS", Value = principal_tests });
            response.Add(new { Code = "TRANSLATESTATUSPATIENTTRANSFER1ANS", Value = patient_status_at_transfer });
            response.Add(new { Code = "TRANSLATECLINEVOLUANS", Value = history });
            response.Add(new { Code = "TRANSLATESUBRESULTANS", Value = principal_tests });
            response.Add(new { Code = "TRANSLATERECOMMENDATIONANDFOLLOWUPANS", Value = recommendation_and_follow_up });
            response.Add(new { Code = "TRANSLATEREASONTRANSFERANS", Value = reason_for_transfer1 });
            response.Add(new { Code = "TRANSLATENAMEMETHODCONTACTEDANS", Value = contacted_staff });
            response.Add(new { Code = "TRANSLATETRANSPORTFOOTERANS", Value = transportation_method });
            response.Add(new { Code = "TRANSLATEPERTRANSPORTANS", Value = medical_escort });
            if (!opd.IsTelehealth)
            {
                response.Add(new { Code = "TRANSLATEKLSANS", Value = kls });
            }
            response.Add(new { Code = "TRANSLATEPRINCIPALTEST", Value = result_of_paraclinical_tests });
            var data = (
            from md in unitOfWork.MasterDataRepository.Find(x => x.Clinic.Contains("OPD_"+type+"_"+language.ToUpper()) && !x.IsDeleted).Select(md => new {
                md.Code,
                md.Group,
                md.Order,
                md.DataType,
                md.Level,
                md.IsReadOnly,
                md.Note,
                md.Data,
                md.Clinic,
                md.DefaultValue,
                Value = "",
                Id = "",
                md.CreatedAt
            })
             .OrderBy(md => md.CreatedAt)
             .ToList() join res in response on md.Code equals res.Code select new {md.Code,res.Value}).ToList<dynamic>();
            return data;
        }
        private dynamic GetIPDMedicalReport(Guid visit_id, string language, out IPD visit,string type)
        {
            visit = null;
            var ipd = unitOfWork.IPDRepository.GetById(visit_id);
            if (ipd == null)
                return Content(HttpStatusCode.NotFound, Message.IPD_NOT_FOUND);

            visit = ipd;
            List<dynamic> response = new List<dynamic>();

            var customer = ipd.Customer;
            var gender = ConvertGender((int)customer.Gender, language);
            var nationality = customer.Nationality;

            List<string> address = new List<string>();
            if (!string.IsNullOrEmpty(customer.MOHAddress))
                address.Add(customer.MOHAddress);
            if (!string.IsNullOrEmpty(customer.MOHDistrict))
                address.Add(customer.MOHDistrict);
            if (!string.IsNullOrEmpty(customer.MOHProvince))
                address.Add(customer.MOHProvince);

            response.Add(new { Code = "TRANSLATEGENANS", Value = gender });
            response.Add(new { Code = "TRANSLATENATANS", Value = nationality });
            response.Add(new { Code = "TRANSLATEADDANS", Value = string.Join(", ", address) });

            var medical_record = ipd.IPDMedicalRecord;
            var part_2 = medical_record?.IPDMedicalRecordPart2;
            var part_2_datas = part_2?.IPDMedicalRecordPart2Datas.Where(e => !e.IsDeleted).ToList();
            var chief_complaint = part_2_datas?.FirstOrDefault(e => e.Code == "IPDMRPTLDVVANS")?.Value;

            var part_3 = medical_record?.IPDMedicalRecordPart3;
            var part_3_datas = part_3?.IPDMedicalRecordPart3Datas.Where(e => !e.IsDeleted).ToList();
            string clinical_evolution = "";
            try
            {
                clinical_evolution = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPEQTBLANS")?.Value;
            }
            catch (Exception)
            {
                clinical_evolution = "";
            }
            var result_of_paraclinical_tests = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPETTKQANS")?.Value;
            var co_morbidities = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPECDKTANS")?.Value;
            var treatments_and_procedures = (part_3_datas != null && part_3 != null) ? GetTreatmentsLast(ipd.Id, part_3.Id, part_3_datas) : null;
            var significant_medications = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPETCDDANS")?.Value;
            var condition_at_discharge = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPETTNBANS")?.Value;
            var followup_care_plan = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPEHDTVANS")?.Value;
            var doctor_recommendations = part_3_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "IPDMRPEHDTVANS")?.Value;
            var job = customer.MOHJob;
            var workPlace = customer.WorkPlace;
            var medical_record_datas = medical_record?.IPDMedicalRecordDatas.Where(e => !e.IsDeleted).ToList();

            var receiving_hospital = medical_record_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "IPDRH1ANS")?.Value;
            var reason_1 = medical_record_datas?.FirstOrDefault(e => e.Code == "IPDRFT1LOG");
            var reason_1_data = unitOfWork.MasterDataRepository.FirstOrDefault(m => m.Code == "IPDRFT1LOG");
            var reason_2 = medical_record_datas?.FirstOrDefault(e => e.Code == "IPDRFT1SHT");
            var reason_2_data = unitOfWork.MasterDataRepository?.FirstOrDefault(m => m.Code == "IPDRFT1SHT");
            var reason_for_transfer = new List<dynamic>() {
                new { reason_1_data?.ViName, reason_1?.Value},
                new { reason_2_data?.ViName, reason_2?.Value},
            };
            string reason_for_transfer1 = medical_record_datas.FirstOrDefault(e => !e.IsDeleted && e.Code == "IPDRFTANS")?.Value;
            var transportation_method = medical_record_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "IPDTM1ANS")?.Value;
            var escort_person = medical_record_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "IPDNTMANS")?.Value;
            string hospitalizedStatus = part_2_datas?.FirstOrDefault(e => e.Code == "IPDMRPTTTBAANS")?.Value ?? string.Empty;
            var dischargeStatus = part_3_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "IPDMRPETTNBANS")?.Value;

            IPDSurgeryCertificate certificate = unitOfWork.IPDSurgeryCertificateRepository?.FirstOrDefault(x => x.VisitId == visit_id && x.VisitTypeGroupCode == "IPD");
            var dataMap = new List<MappingData>();
            if (certificate != null)
            {
                dataMap = certificate.IPDSurgeryCertificateDatas.Where(e => !e.IsDeleted)
                .Select(e => new MappingData() { Code = e.Code, Value = e.Value, EnValue = e.EnValue }).OrderBy(e => e.Code).ToList();
                if (certificate.FormId != null)
                {
                    Dictionary<string, string> codes = new Dictionary<string, string>()
                        {
                            {"IPDSURCER08", "EIOPS12"},
                            {"IPDSURCER10", "EIOPS60"},
                            {"IPDSURCER04", "EIOPS26"},
                            {"IPDSURCER22", "EIOPS10"},
                            {"IPDSURCER12", "EIOPS62"},
                            {"IPDSURCER14", "EIOPS18"},
                            {"IPDSURCER16", "EIOPS20"},
                            {"IPDSURCER18", "EIOPS42"},
                            {"IPDSURCER20", "EIOPS02"},

                     };

                    Guid? procedureId = certificate.FormId;
                    var procedure = GetProcedureSummary(procedureId);
                    dataMap = AutofillFromProcedure(dataMap, procedure, codes);

                    dataMap = FormatString(dataMap);
                }
            }
            string reportDiagnosis = "";
            if (part_3_datas != null && part_3_datas.Count > 0)
            {
                //Chẩn đoán bệnh chính
                var chanDoanBenhChinh = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPECDBCANS")?.Value;
                string maChanDoanBenhChinh = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPEICDCANS")?.Value;
                List<DiagnosisModel> mainDiagnosisCodes = new List<DiagnosisModel>();
                string mainCodes = "";
                if (maChanDoanBenhChinh != null)
                {
                    mainDiagnosisCodes = JsonConvert.DeserializeObject<List<DiagnosisModel>>(maChanDoanBenhChinh);
                    if (mainDiagnosisCodes != null)
                    {
                        mainCodes = mainDiagnosisCodes[0].code;
                    }
                }

                //Chẩn đoán bệnh kèm theo
                var chanDoanBenhKemTheo = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPECDKTANS")?.Value;
                string maChanDoanBenhKemTheo = part_3_datas?.FirstOrDefault(e => e.Code == "IPDMRPEICDPANS")?.Value;
                string optionCodes = "";
                if (maChanDoanBenhKemTheo != null)
                {
                    List<DiagnosisModel> optionDiagnosisCodes = JsonConvert.DeserializeObject<List<DiagnosisModel>>(maChanDoanBenhKemTheo);
                    if (optionDiagnosisCodes != null)
                    {
                        optionCodes = String.Join(", ", optionDiagnosisCodes.Select(e => e.code).ToArray());
                    }
                }

                if (!string.IsNullOrEmpty(chanDoanBenhChinh) && !string.IsNullOrEmpty(chanDoanBenhKemTheo))
                {
                    chanDoanBenhChinh += ", ";
                }
                if (!string.IsNullOrEmpty(mainCodes) && !string.IsNullOrEmpty(optionCodes))
                {
                    mainCodes += ", ";
                }

                reportDiagnosis = $"{chanDoanBenhChinh}{chanDoanBenhKemTheo} ({mainCodes}{optionCodes})";

                if (string.IsNullOrEmpty(mainCodes) && string.IsNullOrEmpty(optionCodes))
                {
                    reportDiagnosis.Replace(" ()", "");
                }
            }
            var medical_sign = GetMedicalSigByMedicalRecord(ipd.Id, part_2);
            string medicalMign = "";
            if (medical_sign?.Count > 0)
            {
                
                foreach(var m in medical_sign)
                {
                    var value = m.Value ?? "";
                    medicalMign += m.ViName + ": " + value + "\n";
                }
                if(medicalMign.Contains("1. Toàn thân:"))
                {
                    medicalMign.Replace("1. Toàn thân:", "Toàn thân:");
                }
                if (medicalMign.Contains("1. Toàn thân:"))
                {
                    medicalMign.Replace("2. Các cơ quan", "Các cơ quan:");
                }
            }
            var receiving_person = medical_record_datas?.FirstOrDefault(e => !e.IsDeleted && e.Code == "IPDNTMANS")?.Value;
            response.Add(new { Code = "TRANSLATEPREDIAGNOSISANS", Value = dataMap?.FirstOrDefault(x=>x.Code == "IPDSURCER08")?.Value });
            response.Add(new { Code = "TRANSLATEDIAGNOSISAFTERSERGERYANS", Value = dataMap?.FirstOrDefault(x => x.Code == "IPDSURCER22")?.Value });
            response.Add(new { Code = "TRANSLATEPPPTANS", Value = dataMap?.FirstOrDefault(x => x.Code == "IPDSURCER14")?.Value });
            response.Add(new { Code = "TRANSLATEPPVCANS", Value = dataMap?.FirstOrDefault(x => x.Code == "IPDSURCER16")?.Value });
            response.Add(new { Code = "TRANSLATEREASONANS", Value = chief_complaint });
            response.Add(new { Code = "TRANSLATECLINEVOLUANS", Value = clinical_evolution });
            response.Add(new { Code = "TRANSLATERESULTPRATESTANS", Value = result_of_paraclinical_tests });
            response.Add(new { Code = "TRANSLATEPDIAGNOSISANS", Value = reportDiagnosis });
            response.Add(new { Code = "TRANSLATECOMORBIDITIESANS", Value = co_morbidities });
            response.Add(new { Code = "TRANSLATETREATMENTANDPROCEDUREANS", Value = treatments_and_procedures });
            response.Add(new { Code = "TRANSLATEDRUGUSEDANS", Value = significant_medications });
            response.Add(new { Code = "TRANSLATEPERCURRENTPATIENTANS", Value = condition_at_discharge });
            response.Add(new { Code = "TRANSLATEFOLLOWUPCAREPLANANS", Value = followup_care_plan });
            response.Add(new { Code = "TRANSLATENOTEANS", Value = doctor_recommendations });
            response.Add(new { Code = "TRANSLATEJOBANS", Value = job });
            response.Add(new { Code = "TRANSLATEWORKPLACEANS", Value = workPlace });
            response.Add(new { Code = "TRANSLATETREATANDDRUGANS", Value = treatments_and_procedures + "/n" + significant_medications });
            response.Add(new { Code = "TRANSLATECURRENTSATATUSANS", Value = condition_at_discharge });
            response.Add(new { Code = "TRANSLATETRANSPORTANS", Value = transportation_method });
            response.Add(new { Code = "TRANSLATEPERTRANSPORTANS", Value = escort_person });
            response.Add(new { Code = "TRANSLATETREATMENTANS", Value = treatments_and_procedures });
            response.Add(new { Code = "TRANSLATESTATUSINJURYDISCHARGEANS", Value = dischargeStatus });
            response.Add(new { Code = "TRANSLATEFIRSTLASTNAMEPATIENTANS", Value = customer.Fullname });
            response.Add(new { Code = "TRANSLATEGRANDPARENTANS", Value = customer.Fullname });
            response.Add(new { Code = "TRANSLATESUBRESULTANS", Value = result_of_paraclinical_tests });
            response.Add(new { Code = "TRANSLATECONDITIONDISCHARGEANS", Value = condition_at_discharge }); 
            response.Add(new { Code = "TRANSLATCURRENTPATIENTANS", Value = condition_at_discharge });
            response.Add(new { Code = "TRANSLATELABANDSUBRESULTANS", Value = result_of_paraclinical_tests });
            response.Add(new { Code = "TRANSLATEMPTDRUGUSETREATMENTANS", Value = treatments_and_procedures + "\n" + significant_medications});
            response.Add(new { Code = "TRANSLATESATATUSREFERALANS", Value = condition_at_discharge });
            response.Add(new { Code = "TRANSLATETREANTMENTPLANANS", Value = followup_care_plan });
            response.Add(new { Code = "TRANSLATESTATUSADMITTEDANS", Value = hospitalizedStatus });
            response.Add(new { Code = "TRANSLATECHIEFCOMANS", Value = chief_complaint });
            response.Add(new { Code = "TRANSLATESTATUSPATIENTTRANSFERANS", Value = condition_at_discharge });
            response.Add(new { Code = "TRANSLATEDHLSANS", Value = medicalMign });
            response.Add(new { Code = "TRANSLATECAREPLANANS", Value = followup_care_plan });
            response.Add(new { Code = "TRANSLATEREASONTRANSFERANS", Value = reason_for_transfer1 });
            response.Add(new { Code = "TRANSLATESENDERANS", Value = escort_person });
            response.Add(new { Code = "TRANSLATENAMEMETHODCONTACTEDANS", Value = receiving_person });
            response.Add(new { Code = "TRANSLATEGENVIANS", Value = gender });
            response.Add(new { Code = "TRANSLATESENDERFOOTERANS", Value = escort_person });
            response.Add(new { Code = "TRANSLATETRANSPORTFOOTERANS", Value = transportation_method });
            response.Add(new { Code = "TRANSLATEADDFOOTERANSANS", Value = string.Join(", ", address) });
            var data = (
            from md in unitOfWork.MasterDataRepository.Find(x => x.Clinic.Contains("IPD_" + type + "_" + language.ToUpper()) && !x.IsDeleted).Select(md => new {
               md.Code,
               md.Group,
               md.Order,
               md.DataType,
               md.Level,
               md.IsReadOnly,
               md.Note,
               md.Data,
               md.Clinic,
               md.DefaultValue,
               Value = "",
               Id = "",
               md.CreatedAt
            })
            .OrderBy(md => md.CreatedAt)
            .ToList()
           join res in response on md.Code equals res.Code
           select new { md.Code, res.Value }).ToList<dynamic>();
            return data;
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

        protected List<MappingData> AutofillFromProcedure(List<MappingData> datas, EIOProcedureSummary procedure, Dictionary<string, string> codes)
        {
            var codeKeys = codes.Keys.ToList();
            var codeValues = codes.Values.ToList();
            var procedureId = procedure?.Id;
            var dataProcedure = (from d in unitOfWork.EIOProcedureSummaryDataRepository.AsQueryable()
                                 where !d.IsDeleted && d.EIOProcedureSummaryId == procedureId
                                 && codeValues.Contains(d.Code)
                                 select d).ToList();

            foreach (var item in codeKeys)
            {
                var check = datas.FirstOrDefault(e => e.Code == item);
                if (check == null)
                {
                    MappingData _new = new MappingData()
                    {
                        Code = item,
                        Value = null,
                        EnValue = null
                    };
                    datas.Add(_new);
                }
            }

            var dataResult = (from d in datas
                              select new MappingData()
                              {
                                  Code = d.Code,
                                  Value = ChangeValue(d, dataProcedure, codes).Value,
                                  EnValue = ChangeValue(d, dataProcedure, codes).EnValue,
                              }).ToList();

            return dataResult;
        }
        protected MappingData ChangeValue(MappingData data, List<EIOProcedureSummaryData> dataProcedure, Dictionary<string, string> codes)
        {
            var codeKeys = codes.Keys.ToList();
            if (codeKeys.Contains(data.Code))
            {
                string key = data.Code;
                data.Value = dataProcedure.FirstOrDefault(e => e.Code == codes[key]) == null ? "" : dataProcedure.FirstOrDefault(e => e.Code == codes[key]).Value;
                data.EnValue = dataProcedure.FirstOrDefault(e => e.Code == codes[key]) == null ? "" : dataProcedure.FirstOrDefault(e => e.Code == codes[key]).EnValue;
            }
            return data;
        }
        protected EIOProcedureSummary GetProcedureSummary(Guid? id)
        {
            return unitOfWork.EIOProcedureSummaryRepository.FirstOrDefault(e => !e.IsDeleted && e.Id == id);
        }
        private dynamic GetEOCMedicalReport(Guid visit_id, string language,string type)
        {
            var opd = GetEOC(visit_id);
            if (opd == null)
                return Content(HttpStatusCode.NotFound, Message.ED_NOT_FOUND);

            var oen = GetOutpatientExaminationNote(visit_id);

            if (oen == null) return Content(HttpStatusCode.NotFound, Message.FORM_NOT_FOUND);

            List<dynamic> response = new List<dynamic>();

            var customer = opd.Customer;
            var gender = ConvertGender((int)customer.Gender, language);
            var address = customer.Address;
            response.Add(new { Code = "TRANSLATEGENANS", Value = gender });
            response.Add(new { Code = "TRANSLATEADDANS", Value = address });

            var oen_datas = GetFormData(visit_id, oen.Id, "OPDOEN");
            var reason_for_visit = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENCC0ANS")?.Value;
            var hisory_of_present_illness = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENHPIANS")?.Value;
            var clinical_examination_and_findings = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENCEFANS")?.Value;
            var principal_tests = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENPT0ANS")?.Value;
            var diagnosis = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENDD0ANS")?.Value;
            var treatment_plans = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENTP0ANS")?.Value;
            var recommendation_and_follow_up = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENRFUANS")?.Value;
            var reason_for_transfer = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENRFT3ANS")?.Value;
            var transportation_method = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENTM1ANS")?.Value;
            var patient_status = oen_datas?.FirstOrDefault(e => e.Code == "OPDOENPS0ANS")?.Value;
            response.Add(new { Code = "TRANSLATEREASONFORVISITANS", Value = reason_for_visit });
            response.Add(new { Code = "TRANSLATEHISTORYOFPRESENTANS", Value = hisory_of_present_illness });
            response.Add(new { Code = "TRANSLATEKLSANS", Value = clinical_examination_and_findings });
            response.Add(new { Code = "TRANSLATEPRINCIPALTESTANS", Value = principal_tests });
            response.Add(new { Code = "TRANSLATEPDIAGNOSISANS", Value = diagnosis });
            response.Add(new { Code = "TRANSLATETREANTMENTPLANANS", Value = treatment_plans });
            response.Add(new { Code = "TRANSLATERECOMMENDATIONANDFOLLOWUPANS", Value = recommendation_and_follow_up });
            response.Add(new { Code = "TRANSLATENATANS", Value = customer.Nationality });
            response.Add(new { Code = "TRANSLATEREASONTRANSFERANS", Value = reason_for_transfer });
            response.Add(new { Code = "TRANSLATETRANSPORTANS", Value = transportation_method });
            response.Add(new { Code = "TRANSLATEJOBANS", Value = customer.Job });
            response.Add(new { Code = "TRANSLATEWORKPLACEANS", Value = customer.WorkPlace });
            response.Add(new { Code = "TRANSLATESATATUSREFERALANS", Value = patient_status });
            var data = (
            from md in unitOfWork.MasterDataRepository.Find(x => x.Clinic.Contains("EOC_" + type + "_" + language.ToUpper()) && !x.IsDeleted).Select(md => new {
                   md.Code,
                   md.Group,
                   md.Order,
                   md.DataType,
                   md.Level,
                   md.IsReadOnly,
                   md.Note,
                   md.Data,
                   md.Clinic,
                   md.DefaultValue,
                   Value = "",
                   Id = "",
                   md.CreatedAt
            })
           .OrderBy(md => md.CreatedAt)
           .ToList()
           join res in response on md.Code equals res.Code
           select new { md.Code, res.Value }).ToList<dynamic>();
            return data;
        }
        private string ConvertGender(int gen, string lang)
        {
            if (lang.Equals("VI"))
            {
                var gender = "Chưa xác định";
                if (gen == 0)
                    gender = "Nữ";
                else if (gen == 1)
                    gender = "Nam";
                return gender;
            }
            else if (lang.Equals("EN"))
            {
                {
                    var gender = "NA";
                    if (gen == 0)
                        gender = "Female";
                    else if (gen == 1)
                        gender = "Male";
                    return gender;
                }
            }
            return string.Empty;
        }
        private EOCOutpatientExaminationNote GetOutpatientExaminationNote(Guid VisitId)
        {
            var form = unitOfWork.EOCOutpatientExaminationNoteRepository.FirstOrDefault(e => !e.IsDeleted && e.VisitId == VisitId);
            return form;
        }
        private dynamic GetMedicalSigByMedicalRecord(Guid ipdId, IPDMedicalRecordPart2 part_2)
        {
            var medicalOfpatient = (from m in unitOfWork.IPDMedicalRecordOfPatientRepository.AsQueryable()
                                    where !m.IsDeleted && m.VisitId == ipdId
                                    select m).ToList();
            var part3MedicalRecord = (from m in medicalOfpatient
                                      join part3 in unitOfWork.IPDMedicalRecordPart3Repository.AsQueryable()
                                      on m.FormId equals part3.Id
                                      orderby m.UpdatedAt descending
                                      select m).ToList();
            if (part3MedicalRecord == null || part3MedicalRecord.Count == 0) return null;
            var part2_Id = part_2.Id;
            var checkMedicalRecord = part3MedicalRecord[0];
            switch (checkMedicalRecord.FormCode)
            {
                case "A01_038_050919_V":
                    string[] code = new string[]
                     {
                        "IPDMRPT104", "IPDMRPT105", "IPDMRPT106", "IPDMRPT108",
                        "IPDMRPT111", "IPDMRPT113", "IPDMRPT120", "IPDMRPT115",
                        "IPDMRPT116", "IPDMRPT117", "IPDMRPT118", "IPDMRPT119",
                        "IPDMRPTCACQ", "IPDMRPTHOHAANS", "IPDMRPT142",
                        "IPDMRPT122", "IPDMRPT124", "IPDMRPT126", "IPDMRPT144",
                        "IPDMRPT128", "IPDMRPT130", "IPDMRPT132", "IPDMRPT133",
                        "IPDMRPT135", "IPDMRPT137", "IPDMRPTCXNCANS", "IPDMRPTTTBAANS",
                        "IPDMRPT139"
                     };
                    return GetValueFromMasterData(code, part2_Id);

                case "A01_035_050919_V":
                    code = new string[]
                    {
                        "IPDMRPTTTYTANS", "IPDMRPTTUHOANS", "IPDMRPTHOHAANS", "IPDMRPTTIHOANS",
                        "IPDMRPTTTNSANS", "IPDMRPT831"
                    };
                    return GetValueFromMasterData(code, part2_Id);
                case "A01_037_050919_V":
                    code = new string[]
                    {
                        "IPDMRPTTTYTANS", "IPDMRPTCACQANS", "IPDMRPTTUHOANS", "IPDMRPTHOHAANS",
                        "IPDMRPTTIHOANS", "IPDMRPTTTNSANS", "IPDMRPTTHKIANS", "IPDMRPTCOXKANS",
                        "IPDMRPTTAMHANS", "IPDMRPTRAHMANS", "IPDMRPTMMATANS", "IPDMRPTNTDDANS"
                    };
                    return GetValueFromMasterData(code, part2_Id);

                default:
                    code = new string[]
                    {
                        "IPDMRPTTTYTANS", "IPDMRPTTUHOANS", "IPDMRPTTUHOANS", "IPDMRPTHOHAANS",
                        "IPDMRPTTIHOANS", "IPDMRPTTTNSANS", "IPDMRPTTHKIANS", "IPDMRPTCOXKANS",
                        "IPDMRPTTAMHANS", "IPDMRPTRAHMANS", "IPDMRPTMMATANS", "IPDMRPTNTDDANS",
                        "IPDMRPTCACQ",
                     };

                    var medical_sign = (from master in unitOfWork.MasterDataRepository.AsQueryable().Where(
                                    e => !e.IsDeleted &&
                                    !string.IsNullOrEmpty(e.Code) &&
                                    code.Contains(e.Code)
                                )
                                        join data_sql in unitOfWork.IPDMedicalRecordPart2DataRepository.AsQueryable().Where(
                                            e => !e.IsDeleted &&
                                            e.IPDMedicalRecordPart2Id != null &&
                                            e.IPDMedicalRecordPart2Id == part_2.Id &&
                                            !string.IsNullOrEmpty(e.Code) &&
                                            code.Contains(e.Code)
                                        )
                                        on master.Code equals data_sql.Code into data_list
                                        from data_sql in data_list.DefaultIfEmpty()
                                        select new
                                        {
                                            master.Code,
                                            data_sql.Value,
                                            master.ViName,
                                            master.EnName,
                                            master.Order
                                        }).OrderBy(e => e.Order).ToList();
                    return medical_sign;
            }
        }
        private dynamic GetValueFromMasterData(string[] code, Guid part2_Id)
        {
            var medical_sign = (from master in unitOfWork.MasterDataRepository.AsQueryable().Where(
                           e => !e.IsDeleted &&
                           !string.IsNullOrEmpty(e.Code) &&
                           code.Contains(e.Code)
                       )
                                join data_sql in unitOfWork.IPDMedicalRecordPart2DataRepository.AsQueryable().Where(
                                    e => !e.IsDeleted &&
                                    e.IPDMedicalRecordPart2Id != null &&
                                    e.IPDMedicalRecordPart2Id == part2_Id &&
                                    !string.IsNullOrEmpty(e.Code) &&
                                    code.Contains(e.Code)
                                )
                                on master.Code equals data_sql.Code into data_list
                                from data_sql in data_list.DefaultIfEmpty()
                                select new
                                {
                                    master.Code,
                                    Value = data_sql.Value,
                                    master.ViName,
                                    master.EnName,
                                    master.Order
                                }).OrderBy(e => e.Order).ToList();
            List<dynamic> list_data = new List<dynamic>();
            foreach (var item in code)
            {
                var data = (from d in medical_sign
                            where d.Code == item
                            select d).FirstOrDefault();
                if (data == null) continue;
                list_data.Add(data);
            }
            return list_data;
        }

    }
}
