using DataAccess.Models;
using DataAccess.Models.EDModel;
using DataAccess.Models.IPDModel;
using DataAccess.Models.OPDModel;
using EForm.Authentication;
using EForm.BaseControllers;
using EForm.Client;
using EForm.Common;
using EForm.Models;
using EForm.Models.EDModels;
using EForm.Models.IPDModels;
using EForm.Models.OPDModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace EForm.Controllers.GeneralControllers
{
    [SessionAuthorize]

    public class CustomerController : BaseApiController
    {
        [HttpPost]
        [Route("api/Customer")]
        [Permission(Code = "GHISC3")]
        public IHttpActionResult CreateOrUpdateCustomer([FromBody] CustomerEDParameterModel request)
        {
            var exit_customer = unitOfWork.CustomerRepository.Find(e => e.PID == request.PID).FirstOrDefault();
            if (exit_customer == null)
            {
                CreateCustomer(request, null);
            }
            else
            {
                UpdateCustomer(exit_customer, request, null);
            }
            return Content(HttpStatusCode.OK, new { });
        }
        [Route("api/Customer/{PID}")]
        [Permission(Code = "GHISC4")]
        public IHttpActionResult GetCustomerByPidApi(string PID, [FromUri] SearchParameter request)
        {
            Customer customer = new Customer();
            string autoId = Guid.NewGuid().ToString();
            if (PID.Length == autoId.Length)
            {
                Guid visitId = Guid.Parse(PID);
                var visit = GetVisit(visitId);
                customer = GetUpdateOrCreateHisCustomerByPid(visit.Customer.PID);
            }
            else
            {
                // customer = GetLocalOrHisCustomerByPid(PID);
                customer = GetUpdateOrCreateHisCustomerByPid(PID);
            }
            if (customer == null)
            {
                return Content(HttpStatusCode.BadRequest, Message.CUSTOMER_NOT_FOUND);
            }
            if (customer.Address == null)
            {
                customer.Address = $"{customer.MOHAddress} - {customer.MOHDistrict} - {customer.MOHProvince}";
            }

            if (customer.Address.Trim() == "-  -")
            {
                customer.Address = "";
            }

            if (customer.Fork == "Unknown")
            {
                customer.Fork = customer.MOHEthnic;
            }
            if (customer.Job == "Unknown")
            {
                customer.Job = customer.MOHJob;
            }

            int ageByMonth = (DateTime.Now - (DateTime)customer.DateOfBirth).Days / 30;
            //if (ageByMonth < 72 && request.IsGetAgeByMonth == true)
            //{
            //    var customersFromOH = OHClient.searchPatienteOh(new SearchParameter { PID = PID });
            //    if (customersFromOH != null && customersFromOH.Count > 0)
            //    {
            //        var customerInfo = customersFromOH.First();
            //        if (customerInfo != null)
            //        {
            //            customer.Relationship = customerInfo.Relationship;
            //            customer.RelationshipKind = customerInfo.RelationshipKind;
            //            customer.RelationshipAddress = customerInfo.RelationshipAddress;
            //            customer.RelationshipID = customerInfo.RelationshipID;
            //        }
            //    }
            //}

            return Content(HttpStatusCode.OK, new
            {
                customer.Id,
                customer.PID,
                customer.Fullname,
                customer.AgeFormated,
                DateOfBirth = customer.DateOfBirth?.ToString(Constant.DATE_FORMAT),
                customer.Phone,
                customer.Gender,
                customer.Fork,
                customer.IsVip,
                customer.Nationality,
                customer.Address,
                customer.Job,
                customer.WorkPlace,
                customer.Relationship,
                customer.RelationshipContact,
                customer.IsChronic,
                customer.IdentificationCard,
                IssueDate = customer.IssueDate?.ToString(Constant.DATE_FORMAT),
                customer.IssuePlace,
                customer.MOHJob,
                customer.MOHJobCode,
                customer.MOHEthnic,
                customer.MOHEthnicCode,
                customer.MOHNationality,
                customer.MOHNationalityCode,
                customer.MOHAddress,
                customer.MOHProvince,
                customer.MOHProvinceCode,
                customer.MOHDistrict,
                customer.MOHDistrictCode,
                customer.MOHObject,
                customer.MOHObjectOther,
                Age = ageByMonth >= 72 ? (DateTime.Now.Year - customer.DateOfBirth?.Year).ToString() : $"{ageByMonth} tháng/months",
                customer.RelationshipID,
                HealthInsuranceNumber = customer.HealthInsuranceNumber
            });
        }
        [HttpGet]
        [Route("api/CustomerInfor/{type}/{VisitId}")]
        //[Permission(Code = "CUSINF")]
        public IHttpActionResult CustomerInfoAPI(string type, Guid VisitId)
        {
            var visit = GetVisit(VisitId);
            if (visit == null)
                return Content(HttpStatusCode.NotFound, Message.VISIT_NOT_FOUND);
            dynamic diagnosisAndICD = null;
            dynamic diagnosisAndICDPart3 = null;
            dynamic allergy = null;
            dynamic allergy2 = null;
            dynamic treatment = null;
            dynamic vitalsigns = null;
            if (type == "OPD")
            {
                OPD opd = new OPD();
                opd = visit;
                var iafst = opd.OPDInitialAssessmentForShortTerm;
                var iafth = opd.OPDInitialAssessmentForTelehealth;
                var afrsp = opd.EIOAssessmentForRetailServicePatient;
                dynamic vitalsignsForShortTerm = null;
                dynamic vitalsignsForTelehealth = null;
                dynamic vitalsignsForRetailService = null;
                List<dynamic> list_vitalsigns = new List<dynamic>();

                if (iafst != null)
                {
                    var opdInitialAssessmentForShortTerm = iafst?.OPDInitialAssessmentForShortTermDatas;
                    vitalsignsForShortTerm = new
                    {
                        SpO2 = opdInitialAssessmentForShortTerm?.FirstOrDefault(x => x.Code == "OPDIAFSTOPSPOANS")?.Value,
                        BP = opdInitialAssessmentForShortTerm?.FirstOrDefault(x => x.Code == "OPDIAFSTOPBP0ANS")?.Value,
                        Pulse = opdInitialAssessmentForShortTerm?.FirstOrDefault(x => x.Code == "OPDIAFSTOPPULANS")?.Value,
                        RR = opdInitialAssessmentForShortTerm?.FirstOrDefault(x => x.Code == "OPDIAFSTOPRR0ANS")?.Value,
                        T = opdInitialAssessmentForShortTerm?.FirstOrDefault(x => x.Code == "OPDIAFSTOPTEMANS")?.Value,
                        UpdatedAt = iafst.UpdatedAt
                    };
                    list_vitalsigns.Add(vitalsignsForShortTerm);
                }
                if (iafth != null)
                {
                    var opdInitialAssessmentForTelehealth = iafth?.OPDInitialAssessmentForTelehealthDatas;
                    vitalsignsForTelehealth = new
                    {
                        SpO2 = opdInitialAssessmentForTelehealth?.FirstOrDefault(x => x.Code == "OPDIAFTPPULANS")?.Value,
                        BP = opdInitialAssessmentForTelehealth?.FirstOrDefault(x => x.Code == "OPDIAFTPBP0ANS")?.Value,
                        Pulse = opdInitialAssessmentForTelehealth?.FirstOrDefault(x => x.Code == "OPDIAFTPTEMANS")?.Value,
                        RR = opdInitialAssessmentForTelehealth?.FirstOrDefault(x => x.Code == "OPDIAFTPSPOANS")?.Value,
                        T = opdInitialAssessmentForTelehealth?.FirstOrDefault(x => x.Code == "OPDIAFTPRR0ANS")?.Value,
                        UpdatedAt = iafth.UpdatedAt
                    };
                    list_vitalsigns?.Add(vitalsignsForTelehealth);
                }
                if (afrsp != null)
                {
                    var opdInitialAssessmentForRetailService = afrsp?.EIOAssessmentForRetailServicePatientDatas;
                    vitalsignsForRetailService = new
                    {
                        SpO2 = opdInitialAssessmentForRetailService?.FirstOrDefault(x => x.Code == "EDAFRSPPULANS")?.Value,
                        BP = opdInitialAssessmentForRetailService?.FirstOrDefault(x => x.Code == "EDAFRSPBP0ANS")?.Value,
                        Pulse = opdInitialAssessmentForRetailService?.FirstOrDefault(x => x.Code == "EDAFRSPTEMANS")?.Value,
                        RR = opdInitialAssessmentForRetailService?.FirstOrDefault(x => x.Code == "EDAFRSPSPOANS")?.Value,
                        T = opdInitialAssessmentForRetailService?.FirstOrDefault(x => x.Code == "EDAFRSPRR0ANS")?.Value,
                        UpdatedAt = afrsp.UpdatedAt
                    };
                    vitalsigns = vitalsignsForRetailService;
                    list_vitalsigns.Add(vitalsignsForRetailService);
                }
                if (list_vitalsigns != null && list_vitalsigns.Count > 0)
                {
                    list_vitalsigns = list_vitalsigns.OrderByDescending(x => x.UpdatedAt).ToList();
                    vitalsigns = list_vitalsigns.FirstOrDefault();
                }
                var opdopen = opd.OPDOutpatientExaminationNote;
                diagnosisAndICD = GetVisitDiagnosisAndICD(opd.Id, type, true, getForAnesthesia: (opd.IsAnesthesia ? true : false));
                allergy = getOPDAllergyModel(new OPDQueryModel() { CustomerIsAllergy = null }, VisitId);
                if (opdopen != null)
                {
                    var op = opdopen.OPDOutpatientExaminationNoteDatas;
                    treatment = op?.FirstOrDefault(x => x.Code == "OPDOENTP0ANS")?.Value;
                }
            }
            else
            {
                diagnosisAndICD = GetVisitDiagnosisAndICD(visit.Id, type, false);                
                if (type == "IPD")
                {
                    diagnosisAndICDPart3 = GetVisitDiagnosisAndICD(visit.Id);
                    IPD ipd = new IPD();
                    ipd = visit;
                    var ia = ipd.IPDInitialAssessmentForAdult;
                    if (ia != null)
                    {
                        allergy = ia.IPDInitialAssessmentForAdultDatas.Where(e => !e.IsDeleted && e.Code.StartsWith("IPDIAAUALLE"))
                        .Select(e => new { e.Id, e.Code, e.Value });
                        vitalsigns = new
                        {
                            SpO2 = ia.IPDInitialAssessmentForAdultDatas?.FirstOrDefault(x => x.Code == "IPDIAAUSPO2ANS")?.Value,//IPDIAAUSPO2ANS
                            BP = ia.IPDInitialAssessmentForAdultDatas?.FirstOrDefault(x => x.Code == "IPDIAAUBLPRANS")?.Value,
                            Pulse = ia.IPDInitialAssessmentForAdultDatas?.FirstOrDefault(x => x.Code == "IPDIAAUPULSANS")?.Value,
                            RR = ia.IPDInitialAssessmentForAdultDatas?.FirstOrDefault(x => x.Code == "IPDIAAURERAANS")?.Value,
                            T = ia.IPDInitialAssessmentForAdultDatas?.FirstOrDefault(x => x.Code == "IPDIAAUTEMPANS")?.Value,
                        };
                    }
                    allergy2 = getIPDAllergyModel(new IPDQueryModel() { CustomerIsAllergy = null }, ipd.Id);

                    var ipdMedicalRecord = ipd.IPDMedicalRecord;
                    if (ipdMedicalRecord != null)
                    {
                        var medicalRecordPart3Id = ipdMedicalRecord.IPDMedicalRecordPart3Id;
                        treatment = unitOfWork.IPDMedicalRecordPart3DataRepository.FirstOrDefault(e => !e.IsDeleted && e.IPDMedicalRecordPart3Id == medicalRecordPart3Id && e.Code == "IPDMRPEPPDTANS")?.Value;
                    }

                }
                else
                if (type == "ED")
                {
                    ED ed = new ED();
                    ed = visit;
                    treatment = unitOfWork.DischargeInformationDataRepository.FirstOrDefault(e => !e.IsDeleted && e.DischargeInformationId == ed.DischargeInformationId && e.Code == "DI0TAPANS")?.Value;
                    allergy = getEDAllergyModel(new EDQueryModel() { CustomerIsAllergy = null }, ed.Id);
                    var observation_chart = ed.ObservationChart;
                    if (observation_chart != null)
                    {
                        var data_vital = observation_chart.ObservationChartDatas.FirstOrDefault(e => !e.IsDeleted && e.ObservationChartId == observation_chart.Id);
                        if (data_vital != null)
                        {
                            vitalsigns = new
                            {
                                SpO2 = data_vital.SpO2,
                                BP = data_vital.SysBP + "/" + data_vital.DiaBP,
                                Pulse = data_vital.Pulse,
                                RR = data_vital.Resp,
                                T = data_vital.Temperature
                            };
                        }

                    }
                }
                else
                if (type == "EOC")
                {
                    treatment = unitOfWork.FormDatasRepository.FirstOrDefault(e => !e.IsDeleted && e.VisitId == VisitId && e.Code == "OPDOENTP0ANS")?.Value;
                    allergy = getEOCAllergyModel(new OPDQueryModel() { CustomerIsAllergy = null }, visit.Id);
                    vitalsigns = new
                    {
                        SpO2 = unitOfWork.FormDatasRepository.FirstOrDefault(e => !e.IsDeleted && e.VisitId == VisitId && e.Code == "OPDIAFSTOPSPOANS")?.Value,
                        BP = unitOfWork.FormDatasRepository.FirstOrDefault(e => !e.IsDeleted && e.VisitId == VisitId && e.Code == "OPDIAFSTOPBP0ANS")?.Value,
                        Pulse = unitOfWork.FormDatasRepository.FirstOrDefault(e => !e.IsDeleted && e.VisitId == VisitId && e.Code == "OPDIAFSTOPPULANS")?.Value,
                        RR = unitOfWork.FormDatasRepository.FirstOrDefault(e => !e.IsDeleted && e.VisitId == VisitId && e.Code == "OPDIAFSTOPRR0ANS")?.Value,
                        T = unitOfWork.FormDatasRepository.FirstOrDefault(e => !e.IsDeleted && e.VisitId == VisitId && e.Code == "OPDIAFSTOPTEMANS")?.Value,
                    };
                }
            }
            if (vitalsigns == null)
            {
                vitalsigns = new
                {
                    SpO2 = "",
                    BP = "",
                    Pulse = "",
                    RR = "",
                    T = "",
                };
            }
            if (allergy2 == null)
            {
                allergy2 = new
                {
                    Id = "",
                    Code = "",
                    Value = ""
                };
            }
            return Content(HttpStatusCode.OK, new
            {
                DiagnosisAndICD = diagnosisAndICD,
                DiagnosisDischagre = diagnosisAndICDPart3,
                Allergy = allergy,
                Allergy2 = allergy2,
                Treatment = treatment,
                Vitalsigns = vitalsigns,

            });
        }
        private DiagnosisAndICDModel GetVisitDiagnosisAndICD(Guid visit_id)
        {
            IPD visit = GetIPD(visit_id);
            var statuscode = visit.EDStatus.Code;
            if (visit != null && (statuscode == "IPDTF" || statuscode == "IPDDC" || statuscode == "IPDIHT" ||
                statuscode == "IPDUDT" || statuscode == "IPDDD"))
            {
                var medical_record = visit.IPDMedicalRecord;
                if (medical_record != null)
                {
                    var part_3 = visit.IPDMedicalRecord.IPDMedicalRecordPart3;
                    if (part_3 != null)
                    {
                        var data_eon = visit.IPDMedicalRecord.IPDMedicalRecordPart3.IPDMedicalRecordPart3Datas;
                        if (data_eon != null)
                        {
                            var returnData = new DiagnosisAndICDModel
                            {
                                ICD = data_eon.FirstOrDefault(e => e.Code == "IPDMRPEICDCANS")?.Value,
                                Diagnosis = data_eon.FirstOrDefault(e => e.Code == "IPDMRPECDBCANS")?.Value,
                                ICDOption = data_eon.FirstOrDefault(e => e.Code == "IPDMRPEICDPANS")?.Value,
                                DiagnosisOption = data_eon.FirstOrDefault(e => e.Code == "IPDMRPECDKTANS")?.Value
                            };
                            return returnData;
                        }
                    }
                }
            }
            return new DiagnosisAndICDModel { };
        }
    }
}
