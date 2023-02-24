using Clients.E_Sign;
using DataAccess.Models;
using DataAccess.Models.EDModel;
using DataAccess.Models.EIOModel;
using DataAccess.Models.IPDModel;
using DataAccess.Models.OPDModel;
using EForm.Authentication;
using EForm.BaseControllers;
using EForm.Common;
using EForm.Models.IPDModels;
using EForm.Utils;
using EMRModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace EForm.Controllers.ESignControllers
{
    public class ESignController : BaseApiController
    {
        // GET: ESign
        [HttpGet]
        [CSRFCheck]
        [Route("api/esign-GetListForm/{type}/{PID}")]
        public IHttpActionResult GetListForm(string type, string PID)
        {
            var customer = unitOfWork.CustomerRepository.FirstOrDefault(e => !e.IsDeleted && e.PID == PID);
            if (customer == null)
                return Content(HttpStatusCode.NotFound, Message.CUSTOMER_NOT_FOUND);
            var result = new ListFormPIDModel();
            if (type == "OPD")
            {
                result = GetFormOPD(customer);
            }
            if (type == "ED")
            {
                result = GetFormEOC(customer);                
            }
            if (type == "IPD")
            {
               
            }
            if (result == null)
                return Content(HttpStatusCode.NotFound, Message.DATA_NOT_FOUND);
            return Content(HttpStatusCode.OK, result);

        }
        [HttpGet]
        [CSRFCheck]
        [Route("api/esign-GetListForm/{visitid}/{id}/{formCode}")]
        public IHttpActionResult GetDetailForm(Guid visitid, Guid id, string formCode)
        {
            string visitTypeCode = GetCurrentVisitType();
            var visit = GetVisit(visitid, visitTypeCode);
            if (visit == null)
                return Content(HttpStatusCode.NotFound, Message.VISIT_NOT_FOUND);

            return Content(HttpStatusCode.NotFound, Message.VISIT_NOT_FOUND);
        }

        [HttpPost]
        [CSRFCheck]
        [Route("api/esign-checkstatus/{id}")]
        //[Permission(Code = "DRS000xxx7")]
        public IHttpActionResult CheckStatus(Guid id, [FromBody] StatusRequestModel request)
        {
            var resultformSoap = ESignRequest.CheckStatus(id, request);
            var result = JsonConvert.DeserializeObject<StatusModel>(resultformSoap);
            if(result.status == "0") return Content(HttpStatusCode.NotFound, "không có data");
            var list_data = JsonConvert.DeserializeObject<List<StatusModel>>(result.data); 

            return Content(HttpStatusCode.OK, resultformSoap);
        }
        [HttpPost]
        [CSRFCheck]
        [Route("api/esign-cancel_sign/{id}")]
        //[Permission(Code = "DRS000xxx7")]
        public IHttpActionResult CancelSign(Guid id, [FromBody] StatusRequestModel request)
        {
            var resultformSoap = ESignRequest.CancelSign(id, request);          

            return Content(HttpStatusCode.OK, resultformSoap);
        }
        //fill data
        #region ED
        public Dictionary<string, string> FillReportED(dynamic visit, Guid formId, string username, string formCode)
        {                    
            ED ed = visit;
            // thông tin người bệnh
            var customer = ed.Customer;
            DateTime date = DateTime.Now;
            var fullname_patient = customer.Fullname;
            var dateofbirth = customer.DateOfBirth == null? "": customer.DateOfBirth.Value.ToString(Constant.DATE_FORMAT);
            var PID = customer.PID;
            var gender = new CustomerUtil(customer).GetGender();
            var nation = customer.Nationality;
            var address = customer.Address;
            // thông tin từ bệnh án cấp cứu
            var site = ed.Site;
            var spec = ed.Specialty.ViName;

            var etr = ed.EmergencyTriageRecord;
            var emer_record = ed.EmergencyRecord;
            var emer_record_datas = emer_record.EmergencyRecordDatas.ToList();
            var discharge_info = ed.DischargeInformation.DischargeInformationDatas.ToList();
            var doctorfullname = unitOfWork.UserRepository.FirstOrDefault(e => !e.IsDeleted && e.Username == username)?.Fullname;
            var clinicalEvolution = "";
            // quá trình bệnh lý
            var history = emer_record_datas.FirstOrDefault(e => !e.IsDeleted && e.Code == "ER0HISANS")?.Value;// bệnh dử
            var assessment = new EmergencyRecordAssessment(emer_record.Id).GetList();
            clinicalEvolution += !string.IsNullOrEmpty(history) ? "Bệnh sử/History: " + history + "<w:br/>" : "";
            var _assesment = "";
            if (assessment != null && assessment.Count > 0)
            {
                clinicalEvolution += "Thăm khám/Assessment:" + "<w:br/>";
                foreach (var item in assessment)
                {
                    item.Value = item.Value.Replace("-", "<w:br/>" + "-");
                    clinicalEvolution += "+" + item.ViName + "/ " + item.EnName + ":" + "<w:br/>" + item.Value + "<w:br/>";
                    _assesment += "+" + item.ViName + "/ " + item.EnName + ":" + "<w:br/>" + item.Value + "<w:br/>";
                }
            }
            var hour = date.ToString("HH");
            var date_strg = date.ToString(Constant.DATE_FORMAT);
            var admitteddate = ed.AdmittedDate.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND);//ngày vào viện           
            var cheifcom = etr.EmergencyTriageRecordDatas.FirstOrDefault(e => !e.IsDeleted && e.Code == "ETRCC0ANS")?.Value;
            var result = discharge_info.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0RPTANS2")?.Value;
            var treatment = discharge_info.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0TAPANS")?.Value;// phương pháp điều trị
            var follow_up = discharge_info.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0FCPANS")?.Value;// kế hoạch chăm sóc điều trị
            var medications = discharge_info.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0SM0ANS2")?.Value;// thuốc đã dùng
            var condition = discharge_info.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0CS0ANS")?.Value;// tình trạng người bệnh hiện tại
            var morbidities = discharge_info.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0DIAOPT2")?.Value;//bệnh kèm theo
            var icd = discharge_info.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0DIAICD")?.Value;
            var reason_for_transfer = discharge_info.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0RFTANS")?.Value;
            if (!string.IsNullOrEmpty(icd))
            {
                List<DiagnosisModel> mainDiagnosisCodes = new List<DiagnosisModel>();
                mainDiagnosisCodes = JsonConvert.DeserializeObject<List<DiagnosisModel>>(icd);
                if (mainDiagnosisCodes != null)
                {
                    icd = String.Join(", ", mainDiagnosisCodes.Select(e => e.code).ToArray());
                }
            }            
            var subicd = discharge_info.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0DIAOPT")?.Value;
            if (!string.IsNullOrEmpty(subicd))
            {
                List<DiagnosisModel> mainDiagnosisCodes = new List<DiagnosisModel>();
                mainDiagnosisCodes = JsonConvert.DeserializeObject<List<DiagnosisModel>>(subicd);
                if (mainDiagnosisCodes != null)
                {
                    subicd = String.Join(", ", mainDiagnosisCodes.Select(e => e.code).ToArray());
                }
            }
            var dischargedate = ed.DischargeDate;//ngày ra viện             
            Dictionary<string, string> data = new Dictionary<string, string>();
            //báo cáo y tế
            if (formCode == "A01_144_050919_VE")
            {
                data.Add("[CITY]", ed.Site.Province);
                data.Add("[HOUR]", hour);
                data.Add("[MINUTE]", date.ToString("mm"));
                data.Add("[DATE]", date_strg);
                data.Add("[PATIENTNAME]", fullname_patient);
                data.Add("[DOB]", dateofbirth);
                data.Add("[PID]", PID);
                data.Add("[EXAMEDATE]", admitteddate);
                data.Add("[COMPLAINT]", cheifcom);
                data.Add("[CLINICALEVOLUTION]", clinicalEvolution);
                data.Add("[RESULT]", result);
                data.Add("[TREATMENT]", treatment);
                data.Add("[DIAGNOSIS]", GetDiagnosisED(discharge_info, emer_record_datas, ed.Version, "MEDICAL REPORT"));
                data.Add("[STATUS]", discharge_info.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0CS0ANS")?.Value);
                data.Add("[DOCTORNAME]", doctorfullname);
                data.Add("[FOLLOWUP]", follow_up);
            }
            //Báo cáo y tế ra viện
            if(formCode == "A01_143_290922_VE")
            {
                var discharge_date = dischargedate?.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND);
                var list_confirm = unitOfWork.EIOFormConfirmRepository.Find(e => !e.IsDeleted && e.FormId == formId).ToList();
                var fullname_physican = "";
                var fullname_depthead = "";
                var fullname_director = "";
                var user_physican = list_confirm.Where(e => e.ConfirmType == "DISCHARGEMEDICALREPORT_PHY").FirstOrDefault();
                if (user_physican != null)
                {
                    fullname_physican = unitOfWork.UserRepository.FirstOrDefault(e => !e.IsDeleted && (e.Username == user_physican.ConfirmBy || e.Username == username)).Fullname;
                }
                var user_depthead = list_confirm.Where(e => e.ConfirmType == "DISCHARGEMEDICALREPORT_DEP").FirstOrDefault();
                if (user_depthead != null)
                {
                    fullname_depthead = unitOfWork.UserRepository.FirstOrDefault(e => !e.IsDeleted && (e.Username == user_depthead.ConfirmBy || e.Username == username)).Fullname;
                }
                var director = list_confirm.Where(e => e.ConfirmType == "DISCHARGEMEDICALREPORT_DIR").FirstOrDefault();
                if (director != null)
                {
                    fullname_director = unitOfWork.UserRepository.FirstOrDefault(e => !e.IsDeleted && (e.Username == director.ConfirmBy || e.Username == username)).Fullname;
                }
                data.Add("[DEPT]", spec);
                data.Add("[PATIENTNAME]", fullname_patient);  
                data.Add("[DOB]", dateofbirth);
                data.Add("[PID]", PID);
                data.Add("[GENDER]", gender == null? "": gender);
                data.Add("[NATION]", nation == null? "": nation);
                data.Add("[ADDRESS]", address == null ? "" : address );
                data.Add("[ADMISSION]", admitteddate);
                data.Add("[DISCHARGE]", discharge_date);
                data.Add("[COMPLAINT]", cheifcom);
                data.Add("[CLINICALEVOLUTION]", clinicalEvolution);
                data.Add("[RESULT]", result);
                data.Add("[TREATMENT]", treatment);
                data.Add("[DIAGNOSIS]", GetDiagnosisED(discharge_info, emer_record_datas, ed.Version, "MEDICAL REPORT"));
                data.Add("[ICD]", icd);
                data.Add("[MORBIDITIES]", morbidities);
                data.Add("[SUBICD]", subicd);
                data.Add("[MEDICATIONS]", medications == null ? "" : medications);
                data.Add("[CONDITION]", condition ==null?"" : condition);                
                data.Add("[FOLLOWUP]", follow_up);
                data.Add("[PHYSICAN]", fullname_physican == null ? "" : fullname_physican);
                data.Add("[DEPTHEAD]", fullname_depthead == null ? "" : fullname_depthead);
                data.Add("[DIRECTOR]", fullname_director == null ? "" : fullname_director);
                data.Add("[HOUR]", dischargedate.Value.ToString("HH"));
                data.Add("[DATE]", discharge_date);
                data.Add("[PHONESITE]", ed.Site?.PhoneNumber);
            }
            //Giấy chuyển viện
            if(formCode == "A01_145_050919_VE")
            {
                var discharge_date = dischargedate?.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND);
                var list_confirm = unitOfWork.EIOFormConfirmRepository.Find(e => !e.IsDeleted && e.FormId == formId).ToList();
                var fullname_physican = "";
                var fullname_director = "";
                var user_physican = list_confirm.Where(e => e.ConfirmType == "REFERRALLETTER_PIC").FirstOrDefault();
                if (user_physican != null)
                {
                    fullname_physican = unitOfWork.UserRepository.FirstOrDefault(e => !e.IsDeleted && (e.Username == user_physican.ConfirmBy || e.Username == username)).Fullname;
                }
                var director = list_confirm.Where(e => e.ConfirmType == "REFERRALLETTER_DIR").FirstOrDefault();
                if (director != null)
                {
                    fullname_director = unitOfWork.UserRepository.FirstOrDefault(e => !e.IsDeleted && (e.Username == director.ConfirmBy || e.Username == username)).Fullname;
                }
                data.Add("[PATIENTNAME]", fullname_patient);
                data.Add("[DOB]", dateofbirth);
                data.Add("[PID]", PID);
                data.Add("[GENDER]", gender == null ? "" : gender);
                data.Add("[NATION]", nation == null ? "" : nation);
                data.Add("[FROMDATE]", ed.AdmittedDate.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND));
                data.Add("[TODATE]", discharge_date);
                data.Add("[BHYTNO.]", ed.HealthInsuranceNumber);
                data.Add("[EXPIRE]", ed.ExpireHealthInsuranceDate?.ToString(Constant.DATE_FORMAT));
                data.Add("[REASONTRANFER]", reason_for_transfer);
                data.Add("[TIMETRANFER]", discharge_date);
                data.Add("[STAFFCONTACT]", discharge_info.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0NRHANS")?.Value);
                data.Add("[TRANSPORTATION]", discharge_info.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0TM0ANS")?.Value);
                data.Add("[MEDICALESCORT]", discharge_info.FirstOrDefault(e => !e.IsDeleted && e.Code == "DI0NATANS")?.Value);
                data.Add("[REASONADMISSION]", cheifcom);
                data.Add("[CLINICALEVOLUTION]", clinicalEvolution);
                data.Add("[RESULT]", result);
                data.Add("[TREATMENT]", treatment);
                data.Add("[DIAGNOSIS]", GetDiagnosisED(discharge_info, emer_record_datas, ed.Version, "REFERRAL LETTER"));
                data.Add("[MEDICATIONS]", medications == null ? "" : medications);
                data.Add("[STATUSPATIENT]", condition == null ? "" : condition);
                data.Add("[FOLLOWUP]", follow_up);
                data.Add("[FULLNAMEPYSICAN]", fullname_physican == null ? "" : fullname_physican);
                data.Add("[FULLNAMEDIRECTOR]", fullname_director == null ? "" : fullname_director);
                data.Add("[HOURTRAN]", dischargedate.Value.ToString("HH"));
                data.Add("[MINUTETRAN]", dischargedate.Value.ToString("mm"));
                data.Add("[DATETRANFER]", discharge_date);
            }
            //giấy chuyển tuyến
            if(formCode == "A01_167_180220_VE")
            {
                var methods = discharge_info.FirstOrDefault(e => e.Code == "DI0TAPANS")?.Value;
                var drug_main = discharge_info.FirstOrDefault(e => e.Code == "DI0SM0ANS2")?.Value;
                var age = DateTime.Now.Year - customer.DateOfBirth?.Year;
                var nationality = customer.Nationality;
                if (!string.IsNullOrEmpty(nationality))
                    nationality = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(nationality.ToLower());
                var list_confirm = unitOfWork.EIOFormConfirmRepository.Find(e => !e.IsDeleted && e.FormId == formId).ToList();
                var fullname_physican = "";
                var fullname_tranferauthority = "";
                var user_physican = list_confirm.Where(e => e.ConfirmType == "TRANSFERLETTER_PIC").FirstOrDefault();
                if (user_physican != null)
                {
                    fullname_physican = unitOfWork.UserRepository.FirstOrDefault(e => !e.IsDeleted && (e.Username == user_physican.ConfirmBy || e.Username == username)).Fullname;
                }
                var tranferauthority = list_confirm.Where(e => e.ConfirmType == "TRANSFERLETTER_TAU").FirstOrDefault();
                if (tranferauthority != null)
                {
                    fullname_tranferauthority = unitOfWork.UserRepository.FirstOrDefault(e => !e.IsDeleted && (e.Username == tranferauthority.ConfirmBy || e.Username == username)).Fullname;
                }
                var reason_1 = discharge_info.FirstOrDefault(e => e.Code == "DI0RFT1SHT")?.Value;                
                var reason_2 = discharge_info.FirstOrDefault(e => e.Code == "DI0RFT1LOG")?.Value;
                data.Add("[DOH]", site?.LocationUnit + "" + site?.Location);
                data.Add("[SITE]", site?.ViName);
                data.Add("[PID]", PID);
                data.Add("[DEAR]", discharge_info.FirstOrDefault(e => e.Code == "DI0RH1ANS")?.Value);
                data.Add("[LEVEL]", site?.Level);
                data.Add("[PATIENTNAME]", fullname_patient);
                data.Add("[GENDER]", gender == null ? "" : gender);
                data.Add("[AGE]", age.ToString());
                data.Add("[ADDRESS]", customer.Address);
                data.Add("[ETHNIC]", customer.Fork);
                data.Add("[NATION]", nationality);
                data.Add("[OCCUPATION]", customer.Job);
                data.Add("[WORK]", customer.WorkPlace);
                data.Add("[HINO.]", ed.HealthInsuranceNumber);
                data.Add("[HOIVTD]", ed.ExpireHealthInsuranceDate?.ToString(Constant.DATE_FORMAT));
                data.Add("[FROMDATE]", ed.AdmittedDate.ToString(Constant.DATE_FORMAT));
                data.Add("[TODATE]", dischargedate?.ToString(Constant.DATE_FORMAT));
                data.Add("[CLINICALSIGNS]", clinicalEvolution);
                data.Add("[LABTEST]", result);
                data.Add("[DIAGNOSIS]", GetDiagnosisED(discharge_info, emer_record_datas, ed.Version, "TRANSFER LETTER"));
                data.Add("[METHOD]", drug_main != null && drug_main != "" ? methods + ", " + drug_main : methods);
                data.Add("[FULLNAMEPYSICAN]", fullname_physican);
                data.Add("[TRANFERAUTHORITY]", fullname_tranferauthority);
                data.Add("[CONDITION]", condition);
                data.Add("[CIRCLE1]", ((!String.IsNullOrEmpty(reason_1) && reason_1.Contains("True")) ? "①" : "1"));
                data.Add("[CIRCLE2]", ((!String.IsNullOrEmpty(reason_2) && reason_2.Contains("True")) ? "②" : "2"));
                data.Add("[TREATMENT]", treatment);
                data.Add("[DATETRANFER]", dischargedate?.ToString(Constant.DATE_FORMAT));
                data.Add("[TRANSPORTATION]", discharge_info.FirstOrDefault(e => e.Code == "DI0TM1ANS")?.Value);
                data.Add("[MEDICALESCORT]", discharge_info.FirstOrDefault(e => e.Code == "DI0NTMANS")?.Value);
                data.Add("[DATE]", dischargedate.Value.ToString("DD"));
                data.Add("[MONTH]", dischargedate.Value.ToString("MM"));
                data.Add("[YEAR]", dischargedate.Value.ToString("YYYY"));
            }
            //BIÊN BẢN HỘI CHẨN
            if(formCode == "A01_057_050919_VE")
            {
                var jscm = unitOfWork.EIOJointConsultationGroupMinutesRepository.FirstOrDefault(e =>!e.IsDeleted && e.VisitId == ed.Id && e.VisitTypeGroupCode == "ED");
                var data_jscm = jscm.EIOJointConsultationGroupMinutesDatas.Where(e => !e.IsDeleted).ToList();
                var user_head = data_jscm.FirstOrDefault(e => e.Code == "EIOJCGMPBPDANS")?.Value;
                var headdepartment = "";
                if(user_head != null)
                {
                    headdepartment = unitOfWork.UserRepository.FirstOrDefault(e => !e.IsDeleted && e.Username == user_head).Fullname;
                }
                var admitted_date = data_jscm.FirstOrDefault(e => e.Code == "EIOJCGMADATANS");
                var chairman_title = data_jscm.FirstOrDefault(e => e.Code == "EIOJCGMCHMAANS1");
                var secretary_title = data_jscm.FirstOrDefault(e => e.Code == "EIOJCGMSETAANS1");
                var chairman = jscm.Chairman;
                var secretary = jscm.Secretary;
                var mem = "";
                var members = jscm.EIOJointConsultationGroupMinutesMembers
               .Where(e => !e.IsDeleted && e.IsConfirm)
               .Select(e => new { e.Member?.Id, e.Member?.Fullname, e.Member?.Username });
                if(members != null && members.Count() > 0)
                {
                    foreach(var item in members)
                    {
                        mem += item.Fullname + "<w:br/>";
                    }
                }
                var date_jscm = data_jscm.FirstOrDefault(e => e.Code == "EIOJCGMJCTANS").Value.Trim();
                var including = "";
                var diagnosis = data_jscm.FirstOrDefault(e => e.Code == "EIOJCGMDIAGANS");
                if(diagnosis != null) including += "Chẩn đoán/diagnosis: " + diagnosis.Value + "<w:br/>";                
                var planofcare = data_jscm.FirstOrDefault(e => e.Code == "EIOJCGMPOCAANS");
                if(planofcare != null) including += "Điều trị và chăm sóc/plan of care: " + planofcare.Value + "<w:br/>";                
                var explore = data_jscm.FirstOrDefault(e => e.Code == "EIOJCGMEXPLANS");
                if(explore != null) including += "Thăm dò/Expore: " + explore.Value + "<w:br/>";
                var other = data_jscm.FirstOrDefault(e => e.Code == "EIOJCGMOTHEANS");
                if(other != null) including += "Khác/other: " + other.Value + "<w:br/>";
                var typeofconsultation = "";
                var khoa = data_jscm.FirstOrDefault(e => e.Code == "EIOJCGMTOJCDEP");
                if(khoa != null && khoa.Value == "True") typeofconsultation += "Khoa" + "<w:br/>";
                var lienkhoa = data_jscm.FirstOrDefault(e => e.Code == "EIOJCGMTOJCIDE");
                if (lienkhoa != null && lienkhoa.Value == "True") typeofconsultation += "Liên Khoa" + "<w:br/>";
                var toanbenhvien = data_jscm.FirstOrDefault(e => e.Code == "EIOJCGMTOJCHOS");
                if (toanbenhvien != null && toanbenhvien.Value == "True") typeofconsultation += "Toàn bệnh viện" + "<w:br/>";
                var lienbenhvien = data_jscm.FirstOrDefault(e => e.Code == "EIOJCGMTOJCIHO");
                if (lienbenhvien != null && lienbenhvien.Value == "True") typeofconsultation += "Liên bệnh viện" + "<w:br/>";
                var moichuyengia = data_jscm.FirstOrDefault(e => e.Code == "EIOJCGMTOJCPEE");
                if (moichuyengia != null && moichuyengia.Value == "True") typeofconsultation += "Mời chuyên gia" + "<w:br/>";
                var khac = data_jscm.FirstOrDefault(e => e.Code == "EIOJCGMTOJCOTH");
                if (khac != null && khac.Value == "True") typeofconsultation += "Khác" + "<w:br/>";
                data.Add("[HEADDEPARTMENT]", headdepartment);
                data.Add("[DEPT]", spec);
                data.Add("[PID]", PID);
                data.Add("[DOB]", dateofbirth);
                data.Add("[TYPECONSULTATION]", typeofconsultation);
                data.Add("[REASONCONSULTATION]", data_jscm.FirstOrDefault(e => e.Code == "EIOJCGMROJC")?.Value);
                data.Add("[PATIENTNAME]", fullname_patient);
                data.Add("[GENDER]", gender == null ? "" : gender);///10:55 08/02/2023
                data.Add("[HOUR]", date_jscm !=null? date_jscm.Substring(0,2): "");
                data.Add("[MINUTE]", date_jscm != null ? date_jscm.Substring(3, 2): "");
                data.Add("[DATE]", date_jscm != null ? date_jscm.Substring(5, 3):"");
                data.Add("[MONTH]", date_jscm != null ? date_jscm.Substring(9, 2):"");
                data.Add("[YEAR]", date_jscm != null ? date_jscm.Substring(12, 4):"");
                data.Add("[CHAIRMAN]", chairman?.Fullname + "-" + chairman_title);
                data.Add("[SECRETARY]", secretary?.Fullname + "-" + secretary_title);
                data.Add("[MEMBERS]", mem);
                data.Add("[ETHNIC]", customer.Fork);
                data.Add("[OCCUPATION]", customer.Job);
                data.Add("[HOURADMITTED]", ed.AdmittedDate.ToString("HH"));
                data.Add("[MINUTEADMITTED]", ed.AdmittedDate.ToString("mm"));//.Value.ToString("mm")
                data.Add("[DATEADMITTED]", ed.AdmittedDate.ToString("dd"));
                data.Add("[MONTHADMITTED]", ed.AdmittedDate.ToString("MM"));
                data.Add("[YEARADMITTED]", ed.AdmittedDate.ToString("yyyyy"));
                data.Add("[SUMMARYCONDITION]", data_jscm.FirstOrDefault(e => e.Code == "EIOJCGMSOPCANS")?.Value);
                data.Add("[ISSUES]", data_jscm.FirstOrDefault(e => e.Code == "EIOJCGMITBDANS")?.Value);
                data.Add("[INCLUDINGDIAGNOSIS]", including);
                data.Add("[FULLNAMESECRETARY]", secretary == null? "" :secretary.Fullname);
                data.Add("[FULLNAMECHAIRMAN]", chairman == null?"": chairman.Fullname);
                data.Add("[FULLNAMEMEMBERS]", mem);               
            }
            //giấy chứng nhận thương tích
            if (formCode == "A01_170_050919_V")
            {
                var ic = unitOfWork.EDInjuryCertificateRepository.FirstOrDefault(
                e => !e.IsDeleted &&
                e.VisitId != null &&
                e.VisitId == ed.Id);
                if (ic == null)
                {
                    ic = new EDInjuryCertificate() { VisitId = ed.Id };
                    unitOfWork.EDInjuryCertificateRepository.Add(ic);
                    unitOfWork.Commit();
                }
                var doctor = ic.Doctor;
                var head_of_dept = ic.HeadOfDept;
                var director = ic.Director;
                data.Add("[PROVINCE]", ed.Site.Province);
                data.Add("[SITE]", site.ViName);
                data.Add("[PID]", PID);
                data.Add("[DOB]", dateofbirth);                
                data.Add("[PATIENTNAME]", fullname_patient);
                data.Add("[GENDER]", gender == null ? "" : gender);
                data.Add("[OCCUPATION]", customer.Job);
                data.Add("[WORK]", customer.WorkPlace);
                data.Add("[CCCD]", customer.IdentificationCard);
                data.Add("[ISSUANCEDATE]", customer.IssueDate?.ToString(Constant.DATE_FORMAT));
                data.Add("[PLACEOFISSUE]", customer.IssuePlace);
                data.Add("[ADDRESS]", customer.Address);
                data.Add("[HOURADMITTED]", ed.AdmittedDate.ToString("HH"));
                data.Add("[MINUTEADMITTED]", ed.AdmittedDate.ToString("mm"));
                data.Add("[DATEADMITTED]", ed.AdmittedDate.ToString("dd"));
                data.Add("[MONTHADMITTED]", ed.AdmittedDate.ToString("MM"));
                data.Add("[YEARADMITTED]", ed.AdmittedDate.ToString("yyyy"));
                data.Add("[HOURDISCHARGE]", ed.DischargeDate.Value.ToString("HH"));
                data.Add("[MINUTEDISCHARGE]", ed.DischargeDate.Value.ToString("mm"));
                data.Add("[DATEDISCHARGE]", ed.DischargeDate.Value.ToString("dd"));
                data.Add("[MONTHDISCHARGE]", ed.DischargeDate.Value.ToString("MM"));
                data.Add("[YEARDISCHARGE]", ed.DischargeDate.Value.ToString("yyyy"));
                data.Add("[REASONADMISSION]", cheifcom);
                data.Add("[DIAGNOSIS]", GetDiagnosisED(discharge_info, emer_record_datas, ed.Version, "INJURY CERTIFICATE"));
                data.Add("[TREATMENT]", treatment);
                data.Add("[INJURYSITUATIONADMITTED]", _assesment);
                data.Add("[INJURYSITUATIONDISCHARGE]", condition);
                data.Add("[DIRECTOR]", director == null? "": director.Fullname);
                data.Add("[HEADDEPARTMENT]", head_of_dept == null? "": head_of_dept.Fullname);
                data.Add("[FULLNAMEPYSICAN]", doctor == null ? "" : doctor.Fullname);
            }
            //XNTC đông máu ACT Catridge Kaolin
            if(formCode == "A03_041_080322_V")
            {
                var result_kaolin = unitOfWork.FormDatasRepository.FirstOrDefault(e =>!e.IsDeleted && e.VisitId == ed.Id && e.FormId == formId && e.Code == "HLQDMACT02") ;
                var fullname_technician = "";
                var fullname_physician = "";
                var confirminfo = GetFormConfirms(formId);
                var username_technician = confirminfo.FirstOrDefault(e => e.ConfirmType == "Technician_UserCreated");
                if(username_technician != null)
                {
                    fullname_technician = unitOfWork.UserRepository.FirstOrDefault(e => !e.IsDeleted && e.Username == username_technician.ConfirmBy).Fullname;
                }
                var username_physician = confirminfo.FirstOrDefault(e => e.ConfirmType == "ReferringPhysician");
                if (username_physician != null)
                {
                    fullname_physician = unitOfWork.UserRepository.FirstOrDefault(e => !e.IsDeleted && e.Username == username_technician.ConfirmBy).Fullname;
                }
                data.Add("[PID]", PID);
                data.Add("[DOB]", dateofbirth);
                data.Add("[PATIENTNAME]", fullname_patient);
                data.Add("[GENDER]", gender == null ? "" : gender);
                data.Add("[DEPT]", spec);
                data.Add("[RESULT]", result_kaolin == null? "": result_kaolin.Value);
                data.Add("[RESULTCATRIGD]", "");
                data.Add("[DATETECHNICIAN]", DateTime.Now.ToString(Constant.DATE_FORMAT));
                data.Add("[DATEPHYSICIAN]", DateTime.Now.ToString(Constant.DATE_FORMAT));
                data.Add("[FULLNAMETECHNICIAN]", fullname_technician);
                data.Add("[FULLNAMEPHYSICIAN]", fullname_physician);
            }
            
            return data;
        }
        #endregion
        
        private ListFormPIDModel GetFormEOC(Customer customer)
        {
            ListFormPIDModel result = new ListFormPIDModel();
            List<ListFormSpecsModel> specs = new List<ListFormSpecsModel>();
            ListFormSpecsModel spec = new ListFormSpecsModel();
            List<ListFormModel> form = new List<ListFormModel>();
            var eocs = unitOfWork.EOCRepository.Find(e => !e.IsDeleted && e.CustomerId == customer.Id).ToList();
            foreach (var eoc in eocs)
            {
                spec.NameSpec = eoc.Specialty.ViName;
                spec.Profiles = GetForm(eoc.Id);
            }
            specs.Add(spec);
            result.FullName = customer.Fullname;
            result.PID = customer.PID;
            result.Specs = specs;
            return result;
        }
        private ListFormPIDModel GetFormOPD(Customer customer)
        {
            ListFormPIDModel result = new ListFormPIDModel();
            List<ListFormSpecsModel> specs = new List<ListFormSpecsModel>();
            ListFormSpecsModel spec = new ListFormSpecsModel();
            List<ListFormModel> form = new List<ListFormModel>();
            var opds = unitOfWork.OPDRepository.Find(e => !e.IsDeleted && e.CustomerId == customer.Id).ToList();
            foreach (var opd in opds)
            {
                spec.NameSpec = opd.Specialty.ViName;
                spec.Profiles = GetForm(opd.Id);
            }
            specs.Add(spec);
            result.FullName = customer.Fullname;
            result.PID = customer.PID;
            result.Specs = specs;
            return result;
        }
        private List<ListFormModel> GetForm( Guid visitId)
        {
            var _form = (from eioform in unitOfWork.EIOFormRepository.AsQueryable().Where(e => !e.IsDeleted && e.VisitId == visitId)
                         join form in unitOfWork.FormRepository.AsQueryable().Where(e => e.VisitTypeGroupCode == "OPD")
                         on eioform.FormCode equals form.Code
                         select new ListFormModel
                         {
                             ViName = form.Name,
                             FormCode = eioform.FormCode,
                             FormId = eioform.Id,
                             VisitId = eioform.VisitId,
                         }).ToList();            
            return _form;
        }
        private int CountForm(Guid id)
        {
            return unitOfWork.EIOFormConfirmRepository.Find(e => !e.IsDeleted && e.FormId == id).Count();
        }
    }
}