using DataAccess.Models;
using DataAccess.Models.EDModel;
using DataAccess.Models.EIOModel;
using DataAccess.Models.IPDModel;
using DataAccess.Models.OPDModel;
using EForm.BaseControllers;
using EForm.Client;
using EForm.Common;
using EForm.Models.EIOModels;
using EForm.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EForm.Controllers.BaseControllers.BaseEIOControllers
{
    public class EIOJointConsultationForApprovalOfSurgeryController : BaseApiController
    {

        protected EIOJointConsultationForApprovalOfSurgery GetJointConsultationForApprovalOfSurgery(Guid id, string visit_type)
        {
            return unitOfWork.EIOJointConsultationForApprovalOfSurgeryRepository.FirstOrDefault(
                e => !e.IsDeleted &&
                e.Id == id &&
                e.VisitTypeGroupCode == visit_type
            );
        }

        protected EIOJointConsultationForApprovalOfSurgery CreateJointConsultationForApprovalOfSurgery(Guid visit_id, string visit_type)
        {
            var consultation = new EIOJointConsultationForApprovalOfSurgery
            {
                VisitId = visit_id,
                VisitTypeGroupCode = visit_type,
                Version = 3 
            };
            unitOfWork.EIOJointConsultationForApprovalOfSurgeryRepository.Add(consultation);
            unitOfWork.Commit();

            return consultation;
        }
        protected List<EIOJointConsultationForApprovalOfSurgeryResponse> GetOrUpdateNewestDataJointConsultation(EIOJointConsultationForApprovalOfSurgery consultation, dynamic visit, string visit_type)
        {
            switch(visit_type)
            {
                case "ED":
                    return GetOrUpdateNewestDataJointConsultationED(consultation, visit);
                case "OPD":
                    return GetOrUpdateNewestDataJointConsultationOPD(consultation, visit);
                case "IPD":
                    return GetOrUpdateNewestDataJointConsultationIPD(consultation, visit);
                default:
                    return GetOrUpdateNewestDataJointConsultationIPD(consultation, visit);
            }    
        }
        private List<EIOJointConsultationForApprovalOfSurgeryResponse> GetOrUpdateNewestDataJointConsultationED(EIOJointConsultationForApprovalOfSurgery consultation, ED ed)
        {
            var datas = consultation.EIOJointConsultationForApprovalOfSurgeryDatas.Where(e => !e.IsDeleted).ToList();

            var etr = ed.EmergencyTriageRecord;
            var addmission_date = GetOrCreateJointConsultationData("EDJCFAOSTOAANS", consultation.Id, datas);
            addmission_date.Value = etr.TriageDateTime.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND);
            unitOfWork.EIOJointConsultationForApprovalOfSurgeryDataRepository.Update(addmission_date);

            var chief_complain = GetOrCreateJointConsultationData("EDJCFAOSCC0ANS", consultation.Id, datas);
            chief_complain.Value = etr.EmergencyTriageRecordDatas.FirstOrDefault(e => !e.IsDeleted && e.Code == "ETRCC0ANS")?.Value;
            unitOfWork.EIOJointConsultationForApprovalOfSurgeryDataRepository.Update(chief_complain);


            var emer = ed.EmergencyRecord;
            var emer_data = emer.EmergencyRecordDatas;
            var past_health_history = GetOrCreateJointConsultationData("EDJCFAOSPHHANS", consultation.Id, datas);
            past_health_history.Value = emer_data.FirstOrDefault(e => !e.IsDeleted && e.Code == "ER0PHHANS")?.Value;
            unitOfWork.EIOJointConsultationForApprovalOfSurgeryDataRepository.Update(past_health_history);

            var history = GetOrCreateJointConsultationData("EDJCFAOSHISANS", consultation.Id, datas);
            history.Value = emer_data.FirstOrDefault(e => !e.IsDeleted && e.Code == "ER0HISANS")?.Value;
            unitOfWork.EIOJointConsultationForApprovalOfSurgeryDataRepository.Update(past_health_history);

            var assessment = GetOrCreateJointConsultationData("EDJCFAOSASSANS", consultation.Id, datas);
            assessment.Value = new EmergencyRecordAssessment(emer.Id).GetString();
            unitOfWork.EIOJointConsultationForApprovalOfSurgeryDataRepository.Update(assessment);

            var di = ed.DischargeInformation;
            var discharge_info_datas = di.DischargeInformationDatas;
            var diagnosis = GetOrCreateJointConsultationData("EDJCFAOSDIAANS", consultation.Id, datas);

            var discharge_diagnosis = discharge_info_datas.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0DIAANS")?.Value;
            var discharge_diagnosis_other = discharge_info_datas.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0DIAOPT2")?.Value;
            string strdiagnosis = discharge_diagnosis + "/ " + discharge_diagnosis_other;

            var icd10_main_code_list = new List<string>();
            var icd10_secondary_code_list = new List<string>();

            var icd10_main = discharge_info_datas.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0DIAICD")?.Value;
            var icd10_secondary = discharge_info_datas.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0DIAOPT")?.Value;

            icd10_main_code_list.AddRange(ICDConvert.Operate(icd10_main).Select(e => e.Code));
            icd10_secondary_code_list.AddRange(ICDConvert.Operate(icd10_secondary).Select(e => e.Code));

            string icd_10_main_text = "";
            if (icd10_main_code_list.Count > 0)
            {
                icd_10_main_text = string.Join(", ", icd10_main_code_list);
            }
            string icd_10_secondary_text = "";
            if (icd10_secondary_code_list.Count > 0)
            {
                icd_10_secondary_text = string.Join(", ", icd10_secondary_code_list);
            }
            if (string.IsNullOrEmpty(icd_10_main_text) && string.IsNullOrEmpty(icd_10_secondary_text))
            {
                diagnosis.Value = $"{strdiagnosis}";
            }
            else
            {
                diagnosis.Value = $"{strdiagnosis} ({icd_10_main_text}/ {icd_10_secondary_text})";
            }
            unitOfWork.EIOJointConsultationForApprovalOfSurgeryDataRepository.Update(diagnosis);

            unitOfWork.Commit();

            return datas.Select(e => new EIOJointConsultationForApprovalOfSurgeryResponse
            {
                Id = e.Id,
                Code = e.Code,
                Value = e.Value,
                EnValue = e.EnValue,
            }).ToList();
        }
        private List<EIOJointConsultationForApprovalOfSurgeryResponse> GetOrUpdateNewestDataJointConsultationIPD(EIOJointConsultationForApprovalOfSurgery consultation, IPD ipd)
        {
            var datas = consultation.EIOJointConsultationForApprovalOfSurgeryDatas.Where(e => !e.IsDeleted).ToList();

            var transfers = new IPDTransfer(ipd).GetListInfo();
            var first_ipd = transfers.FirstOrDefault(e => e.CurrentType == "IPD");
            var addmission_date = GetOrCreateJointConsultationData("EDJCFAOSTOAANS", consultation.Id, datas);
            addmission_date.Value = first_ipd?.CurrentRawDate?.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND);
            unitOfWork.EIOJointConsultationForApprovalOfSurgeryDataRepository.Update(addmission_date);

            var mere = ipd.IPDMedicalRecord;
            if (mere == null)
                return datas.Select(e => new EIOJointConsultationForApprovalOfSurgeryResponse
                {
                    Id = e.Id,
                    Code = e.Code,
                    Value = e.Value,
                    EnValue = e.EnValue
                }).ToList();

            var part2 = mere.IPDMedicalRecordPart2;
            if (part2 == null)
                return datas.Select(e => new EIOJointConsultationForApprovalOfSurgeryResponse
                {
                    Id = e.Id,
                    Code = e.Code,
                    Value = e.Value,
                    EnValue = e.EnValue
                }).ToList();

            var part2_datas = part2.IPDMedicalRecordPart2Datas.Where(e => !e.IsDeleted).ToList();

            var chief_complain = GetOrCreateJointConsultationData("EDJCFAOSCC0ANS", consultation.Id, datas);
            chief_complain.Value = part2_datas.FirstOrDefault(e => !e.IsDeleted && e.Code == "IPDMRPTLDVVANS")?.Value;
            unitOfWork.EIOJointConsultationForApprovalOfSurgeryDataRepository.Update(chief_complain);

            var past_health_history = GetOrCreateJointConsultationData("EDJCFAOSPHHANS", consultation.Id, datas);
            past_health_history.Value = part2_datas.FirstOrDefault(e => !e.IsDeleted && e.Code == "IPDMRPTBATHANS")?.Value;
            unitOfWork.EIOJointConsultationForApprovalOfSurgeryDataRepository.Update(past_health_history);

            var history = GetOrCreateJointConsultationData("EDJCFAOSHISANS", consultation.Id, datas);
            history.Value = part2_datas.FirstOrDefault(e => !e.IsDeleted && e.Code == "IPDMRPTQTBLANS")?.Value;
            unitOfWork.EIOJointConsultationForApprovalOfSurgeryDataRepository.Update(past_health_history);

            var assessment = GetOrCreateJointConsultationData("EDJCFAOSASSANS", consultation.Id, datas);
            assessment.Value = new IPDMedicalRecordAssessment(part2.Id).GetString();
            var ipdMRA = new IPDMedicalRecordAssessment(part2.Id);
            string codeMedicalRecord = ipdMRA.CheckFormCodeMedicalRecord(ipd.Id);
            switch(codeMedicalRecord)
            {
                case "A01_037_050919_V":
                    string[] codes = Constant.IPD_MR_CODE_A01_037_050919_V;
                    assessment.Value = ipdMRA.ConverToStringFromDatas(ipdMRA.GetDatasByCodes(codes));
                    break;
                case "A01_038_050919_V":
                    codes = Constant.IPD_MR_CODE_A01_038_050919_V;
                    string[] codes_edit = new string[] { "IPDMRPT115", "IPDMRPT116", "IPDMRPT117", "IPDMRPT118", "IPDMRPT119" };
                    var datas_edit = ipdMRA.EditDatasByCodes(ipdMRA.GetDatasByCodes(codes), codes_edit);
                    assessment.Value = ipdMRA.ConverToStringFromDatas(datas_edit);
                    break;
                case "A01_035_050919_V":
                    codes = Constant.IPD_MR_CODE_A01_035_050919_V;
                    assessment.Value = ipdMRA.ConverToStringFromDatas(ipdMRA.GetDatasByCodes(codes));
                    break;
                default:
                    break;
            }
            unitOfWork.EIOJointConsultationForApprovalOfSurgeryDataRepository.Update(assessment);
            unitOfWork.Commit();

            var part3 = mere?.IPDMedicalRecordPart3;
            if (part3 == null)
                return datas.Select(e => new EIOJointConsultationForApprovalOfSurgeryResponse 
                { 
                    Id = e.Id, 
                    Code =e.Code,
                    Value = e.Value,
                    EnValue = e.EnValue 
                }).ToList();
            var part3_datas = part3.IPDMedicalRecordPart3Datas.Where(e => !e.IsDeleted).ToList();
            var diagnosis = GetOrCreateJointConsultationData("EDJCFAOSDIAANS", consultation.Id, datas);
            var discharge_diagnosis = part3_datas.FirstOrDefault(e => !e.IsDeleted && e.Code == "IPDMRPECDBCANS")?.Value;
            var icd_code_list = new List<string>();
            var icd_raw = part3_datas.FirstOrDefault(e => !e.IsDeleted && e.Code == "IPDMRPEICDCANS")?.Value;
            icd_code_list.AddRange(ICDConvert.Operate(icd_raw).Select(e => e.Code));
            if (icd_code_list.Count > 0)
                diagnosis.Value = $"{discharge_diagnosis} ({string.Join(", ", icd_code_list)})";
            else
                diagnosis.Value = discharge_diagnosis;
            unitOfWork.EIOJointConsultationForApprovalOfSurgeryDataRepository.Update(diagnosis);
            unitOfWork.Commit();

            return datas.Select(e => new EIOJointConsultationForApprovalOfSurgeryResponse
            {
                Id = e.Id,
                Code = e.Code,
                Value = StringContent(e, ipd),
                EnValue = e.EnValue,
            }).ToList();
        }
        private List<EIOJointConsultationForApprovalOfSurgeryResponse> GetOrUpdateNewestDataJointConsultationOPD(EIOJointConsultationForApprovalOfSurgery consultation, OPD opd)
        {
            var datas = consultation.EIOJointConsultationForApprovalOfSurgeryDatas.Where(e => !e.IsDeleted).ToList();
            // vao vien
            var addmission_date = GetOrCreateJointConsultationData("EDJCFAOSTOAANS", consultation.Id, datas);
            addmission_date.Value = opd?.OPDInitialAssessmentForShortTerm?.AdmittedDate.Value.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND);
            unitOfWork.EIOJointConsultationForApprovalOfSurgeryDataRepository.Update(addmission_date);

            var outpatientExaminationNoteData = opd.OPDOutpatientExaminationNote.OPDOutpatientExaminationNoteDatas.ToList();
            
            // ly do vao vien
            var chief_complain = GetOrCreateJointConsultationData("EDJCFAOSCC0ANS", consultation.Id, datas);
            chief_complain.Value = outpatientExaminationNoteData.FirstOrDefault(e => !e.IsDeleted && e.Code == "OPDOENCC0ANS")?.Value;
            unitOfWork.EIOJointConsultationForApprovalOfSurgeryDataRepository.Update(chief_complain);
            
            // tien su
            var past_health_history = GetOrCreateJointConsultationData("EDJCFAOSPHHANS", consultation.Id, datas);
            past_health_history.Value = outpatientExaminationNoteData.FirstOrDefault(e => !e.IsDeleted && e.Code == "OPDOENPMHANS")?.Value;
            unitOfWork.EIOJointConsultationForApprovalOfSurgeryDataRepository.Update(past_health_history);

            // benh su
            var history = GetOrCreateJointConsultationData("EDJCFAOSHISANS", consultation.Id, datas);
            history.Value = outpatientExaminationNoteData.FirstOrDefault(e => !e.IsDeleted && e.Code == "OPDOENHPIANS")?.Value;
            unitOfWork.EIOJointConsultationForApprovalOfSurgeryDataRepository.Update(past_health_history);

            //tham kham
            string clinicalExamination = "";
            var clinic = opd.Clinic;
            var oen = opd.OPDOutpatientExaminationNote;
            string clinicCode = ClinicalExaminationAndFindings.GetStringClinicCodeUsed(opd);
            var clinical_examination_and_findings = hsClinicalExamination(oen) ? new ClinicalExaminationAndFindings(clinicCode, clinic?.Data, oen.Id, opd.IsTelehealth, oen.Version).GetData() : null;
            
            if (clinical_examination_and_findings != null && clinical_examination_and_findings.Count > 0)
            {
                foreach(var item in clinical_examination_and_findings)
                {
                    clinicalExamination += "+ " + item.ViName + ": " + item.Value + "\n";
                }    
            }
            var assessment = GetOrCreateJointConsultationData("EDJCFAOSASSANS", consultation.Id, datas);
            assessment.Value = clinicalExamination;
            unitOfWork.EIOJointConsultationForApprovalOfSurgeryDataRepository.Update(assessment);

            // chuan doan
            var diagnosis = GetOrCreateJointConsultationData("EDJCFAOSDIAANS", consultation.Id, datas);
            var discharge_diagnosis = outpatientExaminationNoteData.FirstOrDefault(e => !e.IsDeleted && e.Code == "OPDOENDD0ANS")?.Value;
            discharge_diagnosis = !string.IsNullOrEmpty(discharge_diagnosis) ? discharge_diagnosis : "";
            var icd10_main_code_list = new List<string>();
            var icd10_secondary_code_list = new List<string>();

            var icd10_main = outpatientExaminationNoteData.FirstOrDefault(e => !e.IsDeleted && e.Code == "OPDOENICDANS")?.Value;
            var icd10_secondary = outpatientExaminationNoteData.FirstOrDefault(e => !e.IsDeleted && e.Code == "OPDOENICDOPT")?.Value;

            icd10_main_code_list.AddRange(ICDConvert.Operate(icd10_main).Select(e => e.Code));
            icd10_secondary_code_list.AddRange(ICDConvert.Operate(icd10_secondary).Select(e => e.Code));

            string icd_10_main_text = "";
            if (icd10_main_code_list.Count > 0)
            {
                icd_10_main_text = string.Join(", ", icd10_main_code_list);
            }
            string icd_10_secondary_text = "";
            if (icd10_secondary_code_list.Count > 0)
            {
                icd_10_secondary_text = string.Join(", ", icd10_secondary_code_list);
            }
            if(string.IsNullOrEmpty(icd_10_main_text) && string.IsNullOrEmpty(icd_10_secondary_text))
            {
                diagnosis.Value = $"{discharge_diagnosis}";
            }else
            {
                diagnosis.Value = $"{discharge_diagnosis} ({icd_10_main_text}/ {icd_10_secondary_text})";
            }
            unitOfWork.EIOJointConsultationForApprovalOfSurgeryDataRepository.Update(diagnosis);
            unitOfWork.Commit();

            return datas.Select(e => new EIOJointConsultationForApprovalOfSurgeryResponse
            {
                Id = e.Id,
                Code = e.Code,
                Value = e.Value,
                EnValue = e.EnValue,
                PkntVersion = oen.Version
            }).ToList();
        }

        protected EIOJointConsultationForApprovalOfSurgeryData GetOrCreateJointConsultationData(string code, Guid joint_id, List<EIOJointConsultationForApprovalOfSurgeryData> datas)
        {
            EIOJointConsultationForApprovalOfSurgeryData data = datas.FirstOrDefault(e => e.Code == code);
            if (data != null)
                return data;

            data = new EIOJointConsultationForApprovalOfSurgeryData()
            {
                EIOJointConsultationForApprovalOfSurgeryId = joint_id,
                Code = code
            };
            unitOfWork.EIOJointConsultationForApprovalOfSurgeryDataRepository.Add(data);
            datas.Add(data);
            return data;
        }
        protected void HandleUpdateOrCreateJointConsultationData(EIOJointConsultationForApprovalOfSurgery joint_consultation, JToken request_data)
        {
            var joint_consultation_data = joint_consultation.EIOJointConsultationForApprovalOfSurgeryDatas.Where(e => !e.IsDeleted).ToList();
            foreach (var item in request_data)
            {
                var item_code = item.Value<string>("Code");
                if (string.IsNullOrEmpty(item_code)) continue;

                var data = GetOrCreateJointConsultationData(joint_consultation_data, joint_consultation.Id, item_code);
                if (data != null)
                    UpdateJointConsultationData(data, item.Value<string>("Value"));
            }
            joint_consultation.UpdatedBy = GetUser().Username;
            unitOfWork.EIOJointConsultationForApprovalOfSurgeryRepository.Update(joint_consultation);
            unitOfWork.Commit();
        }
        private EIOJointConsultationForApprovalOfSurgeryData GetOrCreateJointConsultationData(List<EIOJointConsultationForApprovalOfSurgeryData> list_data, Guid joint_id, string code)
        {
            EIOJointConsultationForApprovalOfSurgeryData data = list_data.FirstOrDefault(e => !string.IsNullOrEmpty(e.Code) && e.Code == code);
            if (data != null) return data;

            data = new EIOJointConsultationForApprovalOfSurgeryData()
            {
                EIOJointConsultationForApprovalOfSurgeryId = joint_id,
                Code = code
            };
            unitOfWork.EIOJointConsultationForApprovalOfSurgeryDataRepository.Add(data);
            return data;
        }
        private void UpdateJointConsultationData(EIOJointConsultationForApprovalOfSurgeryData data, string value)
        {
            data.Value = value;
            unitOfWork.EIOJointConsultationForApprovalOfSurgeryDataRepository.Update(data);
        }

        protected dynamic SyncReadOnlyResultOfParaclinicalTests(string site_code, string pid, string visit_code)
        {
            List<dynamic> lab_result;
            List<dynamic> xray_result;
            //if (site_code == "times_city")
            //{
            //    lab_result = EHosClient.GetFinalLabResultsByPIDAndVisitCode(pid, visit_code);
            //    xray_result = EHosClient.GetXrayResultsByPIDAndVisitCode(pid, visit_code);
            //}
            //else
            //{

            //}
            var api_code = GetSiteAPICode();
            lab_result = OHClient.GetFinalLabResultsByPIDVisitCodeAndAPICode(pid, visit_code, api_code);
            xray_result = OHClient.GetFinalXrayResultsByPIDAndVisitCode(pid, visit_code);
            return new {
                XetNghiem = lab_result,
                CDHA = xray_result,
                DiagnosticReporting = GetDiagnosticReportingByPid(pid, visit_code)
            };
        }

        protected bool AcceptJointConsultation(User user, EIOJointConsultationForApprovalOfSurgery consultation, string kind)
        {
            var positions = user.PositionUsers.Where(e => !e.IsDeleted).Select(e => e.Position.EnName);
            if (kind == "CMO" && positions.Contains("CMO") && consultation.CMOId == null)
                consultation.CMOId = user.Id;
            else if (kind == "HeadOfDept" && positions.Contains("Head Of Department") && consultation.HeadOfDeptId == null)
                consultation.HeadOfDeptId = user.Id;
            else if (kind == "Anesthetist" && positions.Contains("Doctor") && consultation.AnesthetistId == null)
                consultation.AnesthetistId = user.Id;
            else if (kind == "Surgeon" && positions.Contains("Doctor") && consultation.SurgeonId == null)
                consultation.SurgeonId = user.Id;
            else
                return false;
            unitOfWork.EIOJointConsultationForApprovalOfSurgeryRepository.Update(consultation);
            unitOfWork.Commit();

            return true;
        }

        protected string StringContent(EIOJointConsultationForApprovalOfSurgeryData data, IPD ipd)
        {
            if (data.Code == "EDJCFAOSPHHANS")
            {
                string[] codes = new string[]
                {
                    "IPDMRPT02",
                    "IPDMRPT1058",
                    "IPDMRPT1645"
                };
                string du = GetInforFromMedicalRecord(codes, ipd, data);
                if(!string.IsNullOrEmpty(du))
                {
                    du = du.Contains("+ Dị ứng") ? du.Replace("+ Dị ứng", "Dị ứng") : du;
                    return du;
                }    
                
            }
            if (data.Code == "EDJCFAOSHISANS")
            {
                var part2 = ipd.IPDMedicalRecord?.IPDMedicalRecordPart2;
                if (part2 == null)
                    return data.Value;

                var nguyennhan = (from d in unitOfWork.IPDMedicalRecordPart2DataRepository.AsQueryable()
                                  where !d.IsDeleted && d.IPDMedicalRecordPart2Id == part2.Id &&
                                  d.Code == "IPDMRPT1054"
                                  select d).FirstOrDefault();
                string[] codes_data;
                if (nguyennhan == null || string.IsNullOrEmpty(nguyennhan?.Value))
                    codes_data = new string[]
                    {
                        "IPDMRPT1052", "IPDMRPT1056"
                    };
                else
                    codes_data = new string[]
                    {
                        "IPDMRPT1052", "IPDMRPT1054", "IPDMRPT1056"
                    };
                string codes = GetInforFromMedicalRecord(codes_data, ipd, data);
                if(!string.IsNullOrEmpty(codes))
                {
                    return codes;
                }    
            }
            return data.Value;
        }
        protected List<dynamic> GetDatasByCode(string[] codes, Guid id)
        {
            var datas = (from mas in unitOfWork.MasterDataRepository.AsQueryable()
                         where !mas.IsDeleted && !string.IsNullOrEmpty(mas.Code)
                         && codes.Contains(mas.Code)
                         join
                         da in unitOfWork.IPDMedicalRecordPart2DataRepository.AsQueryable()
                         .Where(
                             d => !d.IsDeleted &&
                             d.IPDMedicalRecordPart2Id == id &&
                             !string.IsNullOrEmpty(d.Code) &&
                             codes.Contains(d.Code)
                         ) on mas.Code equals da.Code
                         into datas_query
                         from data in datas_query.DefaultIfEmpty()
                         select new
                         {
                             ViName = mas.ViName,
                             Code = mas.Code,
                             Value = data.Value
                         }).ToList();
            List<dynamic> datas_res = new List<dynamic>(datas);
            return datas_res;
        }

        protected string GetInforFromMedicalRecord(string[] codes, IPD ipd, EIOJointConsultationForApprovalOfSurgeryData data)
        {
            if (ipd == null)
                return "";
            if (ipd.IPDMedicalRecord == null)
                return "";
            if (ipd.IPDMedicalRecord.IPDMedicalRecordPart2 == null)
                return "";

            var part2 = ipd.IPDMedicalRecord.IPDMedicalRecordPart2;
            var last = GetLastIPDMedicalRecordOfPatients(ipd.Id);
            if (last.FormCode == "A01_041_050919_V")
            {
                var datas = GetDatasByCode(codes, part2.Id);
                StringBuilder str_res = new StringBuilder();
                foreach (var item in codes)
                {
                    string viname = datas.Where(d => d.Code == item).Select(d => d.ViName).FirstOrDefault();
                    if (viname.Contains("Nguyên nhân (nếu có)"))
                    {
                        string vlue = datas.Where(d => d.Code == item).Select(d => d.Value).FirstOrDefault().ToString();
                        if (!string.IsNullOrEmpty(vlue))
                        {
                            string str = $"\n {datas.Where(d => d.Code == item).Select(d => d.ViName).FirstOrDefault()}: {datas.Where(d => d.Code == item).Select(d => d.Value).FirstOrDefault()}";
                            str_res.Append(str);
                        }    
                    }
                    else
                    {
                        string str = $"\n {datas.Where(d => d.Code == item).Select(d => d.ViName).FirstOrDefault()}: {datas.Where(d => d.Code == item).Select(d => d.Value).FirstOrDefault()}";
                        str_res.Append(str);
                    }    
                }

                return str_res.ToString();
            }
            return data?.Value;
        }
    }
}
