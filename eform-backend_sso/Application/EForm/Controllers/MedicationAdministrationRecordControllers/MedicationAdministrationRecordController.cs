﻿using DataAccess.Models;
using DataAccess.Models.EDModel;
using DataAccess.Models.IPDModel;
using DataAccess.Models.MedicationAdministrationRecordModel;
using DataAccess.Models.OPDModel;
using EForm.Authentication;
using EForm.BaseControllers;
using EForm.Client;
using EForm.Common;
using EForm.Models;
using EForm.Models.MedicationAdministrationRecordModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Http;
using static EForm.Common.Constant;

namespace EForm.Controllers.MedicationAdministrationRecordControllers
{
    [SessionAuthorize]
    public class MedicationAdministrationRecordController : BaseApiController
    {
        internal static List<MedicationGroupByVisitCode> groupsByVisitCode = new List<MedicationGroupByVisitCode>();

        [HttpGet]
        [Route("api/MedicationAdministrationRecord/{pid}")]
        [CSRFCheck]
        [Permission(Code = "IPDPRE")]
        public IHttpActionResult GetIPDPrescriptions(string pid, DateTime fromDate, DateTime toDate, string visitType, string visitCode)
        {
            Customer customer = new Customer();
            string currentPID = "";
            if (pid.Length == 36)
            {

                switch (visitType)
                {
                    case "IPD":
                        IPD visitIPD = GetIPD(Guid.Parse(pid));
                        if (visitIPD != null)
                        {
                            customer = visitIPD.Customer;
                            currentPID = customer.PID;
                        }
                        break;
                    case "ED":
                        ED visitED = GetED(Guid.Parse(pid));
                        if (visitED != null)
                        {
                            customer = visitED.Customer;
                            currentPID = customer.PID;
                        }
                        break;
                    case "OPD":
                        OPD visitOPD = GetOPD(Guid.Parse(pid));
                        if (visitOPD != null)
                        {
                            customer = visitOPD.Customer;
                            currentPID = customer.PID;
                        }
                        break;
                }
            }
            else
            {
                currentPID = pid;
            }


            if (fromDate == null)
            {
                fromDate = DateTime.Now;
            }

            if (toDate == null)
            {
                toDate = DateTime.Now.AddDays(1);
            }

            toDate = new DateTime(toDate.Year, toDate.Month, toDate.Day, 13, 0, 0);
            List<IPDPrescriptionModel> listIPDPrescriptions = GetPrescriptions(currentPID, fromDate, toDate);

            var cusomer = GetCustomerByPid(currentPID);
            var current_username = getUsername()?.ToUpper();
            var hasPrimaryDoctorAD = listIPDPrescriptions != null ? listIPDPrescriptions.FirstOrDefault(e => e.PrimaryDoctorAD?.ToUpper() == current_username || e.PrescriberAD?.ToUpper() == current_username) != null : true;
            if (cusomer.IsVip && !IsVIPMANAGE() && !hasPrimaryDoctorAD)
            {
                if (!IsUnlockVipByPid(currentPID, UnlockVipType.MedicationAdministration) && !HasEFORMViSitOpen(currentPID))
                {
                    return Content(HttpStatusCode.Forbidden, new MsgModel()
                    {
                        ViMessage = "Bạn không có quyền truy cập hồ sơ này",
                        EnMessage = "Bạn không có quyền truy cập hồ sơ này"
                    });
                }
            }


            List<IPDPrescriptionModel> nomalPrecriptions = new List<IPDPrescriptionModel>();
            List<IPDPrescriptionModel> listThucPhamChucNang = new List<IPDPrescriptionModel>();

            MedicationInfoModel medicationInfo = new MedicationInfoModel();

            HashSet<string> codes = new HashSet<string>();

            List<string> prescriptionCodes = new List<string>();

            List<MedicationGroupByVisitCode> listGroupByVisitCode = new List<MedicationGroupByVisitCode>();
            if (listIPDPrescriptions != null && listIPDPrescriptions.Count > 0)
            {
                foreach (var item in listIPDPrescriptions)
                {
                    codes.Add(item.PrescriptionCode);
                }

                prescriptionCodes = codes.OrderByDescending(x => Convert.ToInt64(x)).ToList();

                listIPDPrescriptions.Sort((x, y) => String.Compare(x.OrderReferenceNumber, y.OrderReferenceNumber));

                var iPdPrescriptionsGroupById = listIPDPrescriptions.GroupBy(item => item.OrderReferenceNumber).ToList();
                foreach (var group in iPdPrescriptionsGroupById)
                {
                    IPDPrescriptionModel iPDPrescriptionModel = new IPDPrescriptionModel();
                    iPDPrescriptionModel = group.FirstOrDefault();

                    List<IPDPrescriptionModel> test = group.ToList();

                    test = test.OrderBy(x => x.AdministrationDateTime).ToList();
                    HashSet<string> caSang = new HashSet<string>();
                    HashSet<string> caToi = new HashSet<string>();
                    foreach (IPDPrescriptionModel item in test)
                    {
                        caSang.Add($"<span>{item.CaSang}</span><br>");
                        caToi.Add($"<span>{item.CaToi}</span><br>");
                    }

                    List<string> strCaSang = caSang.ToList();
                    for (int i = 0; i < strCaSang.Count; i++)
                    {
                        if (strCaSang[i].IndexOf(":") > 0)
                        {
                            strCaSang[i] = strCaSang[i].Remove(strCaSang[i].IndexOf(":") + 3, 3);
                        }
                    }

                    List<string> strCaToi = caToi.ToList();
                    for (int i = 0; i < strCaToi.Count; i++)
                    {
                        if (strCaToi[i].IndexOf(":") > 0)
                        {
                            strCaToi[i] = strCaToi[i].Remove(strCaToi[i].IndexOf(":") + 3, 3);
                        }
                    }

                    iPDPrescriptionModel.Shift01 = String.Join("", strCaSang);
                    iPDPrescriptionModel.Shift02 = String.Join("", strCaToi);
                    nomalPrecriptions.Add(iPDPrescriptionModel);
                }

                IPDPrescriptionModel prescription = listIPDPrescriptions[0];

                if (pid.Length == 36)
                {
                    Guid customerId = Guid.Parse(pid);
                    switch (visitType)
                    {
                        case "IPD":
                            medicationInfo = GetIPDMedicalInfo(prescription.VisitCode, prescription.PrimaryDoctorAD, prescription.HospitalCode, customerId);
                            break;
                        case "ED":
                            medicationInfo = GetEDMedicalInfo(prescription.VisitCode, prescription.PrimaryDoctorAD, prescription.HospitalCode, customerId);
                            break;
                        case "OPD":
                            medicationInfo = GetOPDMedicalInfo(prescription.VisitCode, prescription.PrimaryDoctorAD, prescription.HospitalCode, customerId);
                            break;
                    }

                }
                else
                {
                    if (visitCode == null)
                    {
                        visitCode = listIPDPrescriptions[0].VisitCode;
                    }

                    List<IPD> listIpds = unitOfWork.IPDRepository.AsQueryable()
                        .Where(e => e.VisitCode.Contains(visitCode)).ToList();
                    List<ED> listEds = unitOfWork.EDRepository.AsQueryable()
                        .Where(e => e.VisitCode.Contains(visitCode)).ToList();
                    List<OPD> listOpds = unitOfWork.OPDRepository.AsQueryable()
                        .Where(e => e.VisitCode.Contains(visitCode)).ToList();

                    if (listIpds.Count > 0)
                    {
                        visitType = "IPD";
                    }
                    else if (listEds.Count > 0)
                    {
                        visitType = "ED";
                    }
                    else if (listOpds.Count > 0)
                    {
                        visitType = "OPD";
                    }

                    switch (visitType)
                    {
                        case "IPD":
                            medicationInfo = GetIPDMedicalInfo(visitCode, prescription.PrimaryDoctorAD, prescription.HospitalCode, Guid.Empty);
                            break;
                        case "ED":
                            medicationInfo = GetEDMedicalInfo(visitCode, prescription.PrimaryDoctorAD, prescription.HospitalCode, Guid.Empty);
                            break;
                        case "OPD":
                            medicationInfo = GetOPDMedicalInfo(visitCode, prescription.PrimaryDoctorAD, prescription.HospitalCode, Guid.Empty);
                            break;
                        default:
                            medicationInfo = GetIPDMedicalInfo(prescription.VisitCode, prescription.PrimaryDoctorAD, prescription.HospitalCode, Guid.Empty);
                            break;
                    }

                }

                foreach (var item in nomalPrecriptions)
                {
                    item.HospitalInfo = GetHospitalInfo(item.HospitalCode);
                }


                foreach (var item in nomalPrecriptions)
                {
                    if (item.DType == "TP")
                    {
                        listThucPhamChucNang.Add(item);
                    }
                }

                nomalPrecriptions.RemoveAll(item => item.DType == "TP");
            }
            else
            {
                listIPDPrescriptions = new List<IPDPrescriptionModel>();
            }

            List<MedicationAdministrationRecordModel> listMedicationAdministrationRecord = new List<MedicationAdministrationRecordModel>();
            if (visitCode != " ")
            {
                if (nomalPrecriptions.Count > 0)
                {
                    string PID = nomalPrecriptions[0].PID;
                    listMedicationAdministrationRecord = unitOfWork.MedicationAdministrationRecordRepository.AsQueryable().Where(e => e.PID == PID).ToList();
                }
                else if (listThucPhamChucNang.Count > 0)
                {
                    string PID = listThucPhamChucNang[0].PID;
                    listMedicationAdministrationRecord = unitOfWork.MedicationAdministrationRecordRepository.AsQueryable().Where(e => e.PID == PID).ToList();
                }
            }
            else
            {
                string PID = nomalPrecriptions[0].PID;
                listMedicationAdministrationRecord = unitOfWork.MedicationAdministrationRecordRepository.AsQueryable().Where(e => e.PID == PID).ToList();
            }

            if (cusomer != null)
            {
                if (cusomer.DateOfBirth != null)
                {
                    Age age = CalculateAge(Convert.ToDateTime(cusomer.DateOfBirth), DateTime.Today);
                    if (age.Years >= 18)
                    {
                        medicationInfo.CustomerMedicationInfo.IsOver18Age = true;
                    }
                    else
                    {
                        medicationInfo.CustomerMedicationInfo.IsOver18Age = false;
                    }
                }
                else
                {
                    medicationInfo.CustomerMedicationInfo.IsOver18Age = false;
                }
            }
            return Content(HttpStatusCode.OK, new
            {
                IPDPrescriptions = nomalPrecriptions,
                PrescriptionCodes = prescriptionCodes,
                MedicationAdministrationRecordInfo = medicationInfo,
                ListGroupByVisitCode = groupsByVisitCode,
                ThucPhamChucNang = listThucPhamChucNang,
                ListMedicationAdministrationRecords = listMedicationAdministrationRecord
            });

        }

        [HttpPost]
        [Route("api/MedicationAdministrationRecord/DietCode/Create")]
        [CSRFCheck]
        [Permission(Code = "MARDIETCODE")]
        public IHttpActionResult CreateDietCodeForMedicationAdministrationRecord(JObject request)
        {
            if (request != null)
            {
                var PID = (string)request["PID"];
                var VisitCode = (string)request["VisitCode"];
                var PrescriptionCode = (string)request["PrescriptionCode"];
                var DietCode = (string)request["DietCode"];

                List<MedicationAdministrationRecordModel> items = unitOfWork.MedicationAdministrationRecordRepository.AsQueryable().Where(e => e.PID == PID && e.VisitCode == VisitCode).ToList();
                if (items.Count > 0)
                {
                    if (items.Count == 1)
                    {
                        var updateItem = items[0];
                        updateItem.DietCode = DietCode;
                        unitOfWork.MedicationAdministrationRecordRepository.Update(updateItem);
                        unitOfWork.Commit();
                        return Content(HttpStatusCode.OK, Message.SUCCESS);
                    }
                    else
                    {
                        MedicationAdministrationRecordModel mar = items.FirstOrDefault(e => e.PrescriptionCode == PrescriptionCode);
                        mar.DietCode = DietCode;
                        unitOfWork.MedicationAdministrationRecordRepository.Update(mar);
                        unitOfWork.Commit();
                        return Content(HttpStatusCode.OK, Message.SUCCESS);
                    }
                }
                else
                {
                    MedicationAdministrationRecordModel newMedicationAdministrationRecordModel = new MedicationAdministrationRecordModel
                    {
                        PID = PID,
                        VisitCode = VisitCode,
                        PrescriptionCode = PrescriptionCode,
                        DietCode = DietCode
                    };

                    unitOfWork.MedicationAdministrationRecordRepository.Add(newMedicationAdministrationRecordModel);
                    unitOfWork.Commit();
                    return Content(HttpStatusCode.OK, Message.SUCCESS);
                }
            }
            return null;
        }

        private List<IPDPrescriptionModel> GetPrescriptions(string pid, Nullable<DateTime> fromDate, Nullable<DateTime> toDate)
        {
            string stringFromDate = "";
            string stringToDate = "";
            if (fromDate != null && toDate != null)
            {
                DateTime newFromDate = (DateTime)fromDate;
                stringFromDate = newFromDate.ToString("yyyy-MM-dd");
                DateTime newToDate = (DateTime)toDate;
                stringToDate = newToDate.ToString("yyyy-MM-dd");
            }

            DateTime tuNgay = new DateTime(fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day, 9, 0, 0);
            DateTime denNgay = new DateTime(toDate.Value.Year, toDate.Value.Month, toDate.Value.Day, 8, 59, 59);

            string urlPostfix = string.Format("/EMRVinmecCom/1.0.0/getIPDPrescriptionByPID?PID={0}&From={1}&To={2}", pid, stringFromDate, stringToDate);

            try
            {
                JToken response = HISClient.RequestAPI(urlPostfix, "Entries", "Entry");
                if (response != null)
                {
                    List<IPDPrescriptionModel> ipdPrescriptions = JsonConvert.DeserializeObject<List<IPDPrescriptionModel>>(response.ToString());

                    List<IPDPrescriptionModel> autoPlannedAdministrationDateTime = new List<IPDPrescriptionModel>();
                    List<IPDPrescriptionModel> onceOnlyPrescriptions = new List<IPDPrescriptionModel>(); //Thuốc dùng 1 lần
                    List<IPDPrescriptionModel> prnPrescriptions = new List<IPDPrescriptionModel>(); //Thuốc PRN
                    List<IPDPrescriptionModel> prnWithPlan = new List<IPDPrescriptionModel>(); //Thuốc PRN có kế hoạch
                    List<IPDPrescriptionModel> prnWithoutPlan = new List<IPDPrescriptionModel>(); // Thuốc PRN không có kế hoạch
                    List<IPDPrescriptionModel> scheduledPrescription = new List<IPDPrescriptionModel>(); 
                    List<IPDPrescriptionModel> prnWithoutPlanAndAdminitrationDateTime = new List<IPDPrescriptionModel>();
                    List<IPDPrescriptionModel> prnWithPlanAndWithoutAdminitrationDateTime = new List<IPDPrescriptionModel>();
                    List<IPDPrescriptionModel> prnWithoutPlanInTime = new List<IPDPrescriptionModel>(); //List thuốc thực hiện trong khoảng ngày y lệnh
                    List<IPDPrescriptionModel> prnWithoutPlanOutTime = new List<IPDPrescriptionModel>(); //List thuốc thực hiện ngoài khoảng ngày y lệnh
                    List<IPDPrescriptionModel> oneOnlyPrescriptionOutDate = new List<IPDPrescriptionModel>(); //List thuốc dùng 1 lần nhưng thời gian thực hiện nằm ngoài khoảng ngày y lệnh

                    //Nhóm thuốc theo visitCode
                    var prescriptionGroupByVisitCode = ipdPrescriptions.GroupBy(item => item.VisitCode).ToList();
                    groupsByVisitCode = new List<MedicationGroupByVisitCode>();
                    foreach (var item in prescriptionGroupByVisitCode)
                    {
                        MedicationGroupByVisitCode medicationGroupByVisitCode = new MedicationGroupByVisitCode();
                        medicationGroupByVisitCode.VisitCode = item.FirstOrDefault().VisitCode;
                        medicationGroupByVisitCode.PatientArea = item.FirstOrDefault().PatientArea;
                        medicationGroupByVisitCode.VisitCreatedDate = item.FirstOrDefault().VisitCreatedDate;
                        medicationGroupByVisitCode.PrimaryDoctorAD = item.FirstOrDefault().PrimaryDoctorAD;
                        groupsByVisitCode.Add(medicationGroupByVisitCode);
                    }

                    //RecurrenceType: "DAILY", "DAYCYCLE", "INTERVAL", "WEEKDAYS", "ONCE", "UNSCHEDULED"
                    #region Xử lý các thuốc Daily, DayCycle, Interval và Weekdays

                    foreach (var item in ipdPrescriptions)
                    {
                        if (item.RecurrenceType.ToUpper() == "DAILY" || item.RecurrenceType.ToUpper() == "DAYCYCLE" || item.RecurrenceType.ToUpper() == "INTERVAL" || item.RecurrenceType.ToUpper() == "WEEKDAYS")
                        {
                            if (item.AdministrationDateTime == null)
                            {
                                item.AdministrationDateTime = item.PlannedAdministrationDateTime;
                            }
                            autoPlannedAdministrationDateTime.Add(item);
                        }
                    }



                    #endregion

                    #region Xử lý các thuốc chỉ dùng 1 lần duy nhất
                    foreach (var item in ipdPrescriptions)
                    {
                        if (item.RecurrenceType.ToUpper() == "ONCE")
                        {
                            if (item.AdministrationDateTime == null)
                            {
                                item.AdministrationDateTime = item.PlannedAdministrationDateTime;
                            }
                            onceOnlyPrescriptions.Add(item);
                        }
                    }

                    foreach (var item in onceOnlyPrescriptions)
                    {
                        //&& item.PlannedAdministrationDateTime < denNgay
                        if (item.AdministrationDateTime > denNgay && item.PlannedAdministrationDateTime < denNgay)
                        {
                            oneOnlyPrescriptionOutDate.Add(item);
                        }
                    }

                    onceOnlyPrescriptions.RemoveAll(item => item.AdministrationDateTime > denNgay);

                    #endregion

                    #region Xử lý thuốc PRN - Thuốc dùng khi cần
                    foreach (var item in ipdPrescriptions)
                    {
                        if (item.RecurrenceType.ToUpper() == "UNSCHEDULED")
                        {
                            prnPrescriptions.Add(item);
                        }
                    }

                    if (prnPrescriptions.Count > 0)
                    {
                        foreach (var item in prnPrescriptions)
                        {
                            //PRN - không có plan cụ thể
                            if (item.RecurrenceCode.Trim().ToLower().StartsWith("ad") || item.RecurrenceCode.Trim().ToLower().StartsWith("prn"))
                            {
                                prnWithoutPlan.Add(item);
                            }

                            //PRN - có plan cụ thể
                            if (item.RecurrenceCode.Trim().ToLower().StartsWith("schedule"))
                            {
                                prnWithPlan.Add(item);
                            }
                        }


                        if (prnWithPlan.Count > 0)
                        {
                            #region Tách riêng các thuốc prnWithPlan chưa thực hiện (AdministrationDateTime = null
                            foreach (var item in prnWithPlan)
                            {
                                if (item.AdministrationDateTime == null)
                                {
                                    prnWithPlanAndWithoutAdminitrationDateTime.Add(item);
                                }
                            }

                            prnWithPlan.RemoveAll(item => item.AdministrationDateTime == null);
                            #endregion

                            if (prnWithPlan.Count > 0)
                            {
                                //prnWithPlan = prnWithPlan.Where(item => item.AdministrationDateTime >= tuNgay && item.AdministrationDateTime <= denNgay).ToList();
                                prnWithPlan = prnWithPlan.Where(item => item.AdministrationDateTime <= denNgay).ToList();
                                prnWithPlan = prnWithPlan.OrderByDescending(item => item.AdministrationDateTime).ToList();
                                var test = prnWithPlan.GroupBy(item => item.PrescriptionId);

                                List<IPDPrescriptionModel> list = new List<IPDPrescriptionModel>();
                                foreach (var item in test)
                                {
                                    list.Add(item.First());
                                }

                                #region Tự động tạo plan cho thuốc dựa vào lần thực hiện đầu tiên + RecurrencePeriod
                                foreach (var item in list)
                                {
                                    if (item.StopDate == null)
                                    {
                                        item.StopDate = toDate.ToString();
                                    }

                                    DateTime startDate = Convert.ToDateTime(item.AdministrationDateTime);
                                    DateTime stopDate = Convert.ToDateTime(item.StopDate);
                                    List<IPDPrescriptionModel> generatePlanPrescriptions = new List<IPDPrescriptionModel>();

                                    do
                                    {
                                        IPDPrescriptionModel newItem = new IPDPrescriptionModel();
                                        CopyProperties(item, newItem);
                                        newItem.PlannedAdministrationDateTime = startDate;
                                        newItem.AdministrationDateTime = startDate;
                                        if (generatePlanPrescriptions.Count >= 1)
                                        {
                                            newItem.AdministrationUserName = "";
                                        }
                                        generatePlanPrescriptions.Add(newItem);
                                        startDate = startDate.AddMinutes(Convert.ToDouble(item.RecurrencePeriod));
                                    } while (startDate.CompareTo(stopDate) < 0);

                                    scheduledPrescription.AddRange(generatePlanPrescriptions);
                                }
                                #endregion
                            }

                        }


                        if (prnWithoutPlan.Count > 0)
                        {
                            foreach (var item in prnWithoutPlan)
                            {

                                if (item.AdministrationDateTime == null)
                                {
                                    prnWithoutPlanAndAdminitrationDateTime.Add(item);
                                }

                            }
                        }

                        //Sau bước này, trong prnWithoutPlan chỉ còn các thuốc đã thực hiện (AdministrationDateTime != null)
                        prnWithoutPlan.RemoveAll(item => item.AdministrationDateTime == null);


                        #region Thuốc PRN không có plan cụ thể, nếu chưa dừng thì vẫn hiển thị trong y lệnh


                        if (prnWithoutPlan.Count > 0)
                        {
                            DateTime startTime = DateTime.ParseExact(stringFromDate + " 09:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                            DateTime endTime = DateTime.ParseExact(stringToDate + " 08:59:59", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                            foreach (var item in prnWithoutPlan)
                            {
                                DateTime adDateTime = (DateTime)item.AdministrationDateTime;
                                string adDateTimeString = adDateTime.ToString("yyyy-MM-dd");
                                //Tách riêng các thuốc thuộc prnWithoutPlan và có thời gian thực hiện nằm trong khoảng ngày lấy y lệnh
                                if (adDateTime >= startTime && adDateTime <= endTime)
                                {
                                    prnWithoutPlanInTime.Add(item);
                                }
                            }

                            prnWithoutPlan.RemoveAll(e => e.AdministrationDateTime >= startTime && e.AdministrationDateTime <= endTime);
                            //Xử lý các thuốc thuộc prnWithoutPlan, có thời gian thực hiện ko nằm trong khoảng ngày y lệnh
                            //nhưng vẫn đang được chỉ định sử dụng (chưa dừng)
                            if (prnWithoutPlan.Count > 0)
                            {
                                var prescriptionGroupById = prnWithoutPlan.GroupBy(item => item.ItemId);
                                foreach (var item in prescriptionGroupById)
                                {
                                    IPDPrescriptionModel iPDPrescription = item.FirstOrDefault();
                                    iPDPrescription.AdministrationDateTime = null;
                                    iPDPrescription.PlannedAdministrationDateTime = null;
                                    prnWithoutPlanOutTime.Add(iPDPrescription);
                                }

                            }

                        }

                        #endregion

                    }

                    #endregion

                    List<IPDPrescriptionModel> finalPrescriptions = new List<IPDPrescriptionModel>();
                    finalPrescriptions.AddRange(autoPlannedAdministrationDateTime);
                    finalPrescriptions.AddRange(onceOnlyPrescriptions);
                    finalPrescriptions.AddRange(prnWithoutPlan);
                    finalPrescriptions.AddRange(scheduledPrescription);
                    finalPrescriptions.AddRange(prnWithoutPlanInTime);
                    finalPrescriptions = finalPrescriptions.Where(item => item.AdministrationDateTime >= tuNgay && item.AdministrationDateTime <= denNgay).ToList();
                    finalPrescriptions.AddRange(prnWithoutPlanAndAdminitrationDateTime);
                    finalPrescriptions.AddRange(prnWithPlanAndWithoutAdminitrationDateTime);
                    finalPrescriptions.AddRange(prnWithoutPlanOutTime);
                    finalPrescriptions.AddRange(oneOnlyPrescriptionOutDate);

                    foreach (var item in finalPrescriptions)
                    {
                        if (item.Usage.Contains("System.IO.FileNotFoundException"))
                        {
                            int start = item.Usage.IndexOf("System.IO.FileNotFoundException");
                            int end = item.Usage.IndexOf("Boolean forceRecompile)");
                            item.Usage.Remove(start, (end - start) + 1);
                        }
                    }
                    return finalPrescriptions;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return new List<IPDPrescriptionModel>();
            }
        }

        private MedicationInfoModel GetIPDMedicalInfo(string visitCode, string primaryDoctorUserName, string hospitalCode, Guid visitId)
        {
            MedicationInfoModel medicationInfo = new MedicationInfoModel();
            Guid userId = GetUserIdByUserName(primaryDoctorUserName);

            IPD ipd = new IPD();

            medicationInfo.HospitalInfo = GetHospitalInfo(hospitalCode);
            if (visitId != Guid.Empty)
            {
                ipd = GetIPD(visitId);
            }
            else
            {
                List<IPD> listIpds = unitOfWork.IPDRepository.Find(e => !e.IsDeleted && e.VisitCode.Contains(visitCode) && e.PrimaryDoctorId != null).ToList();
                if (listIpds != null && listIpds.Count > 0)
                {
                    ipd = listIpds.FirstOrDefault(e => e.PrimaryDoctorId == userId);
                }
            }

            if (ipd != null)
            {
                try
                {
                    //Ngày tháng năm sinh
                    if (ipd.Customer.DateOfBirth != null)
                    {
                        medicationInfo.CustomerMedicationInfo.DateOfBirth = Convert.ToDateTime(ipd.Customer.DateOfBirth).ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        medicationInfo.CustomerMedicationInfo.DateOfBirth = "";
                    }

                    //Số thẻ BHYT
                    if (ipd.HealthInsuranceNumber != null)
                    {
                        medicationInfo.CustomerMedicationInfo.InsuranceCardNo = ipd.HealthInsuranceNumber;
                    }
                    else
                    {
                        medicationInfo.CustomerMedicationInfo.InsuranceCardNo = "";
                    }

                    //Ngày đến hạn thẻ BHYT
                    if (ipd.ExpireHealthInsuranceDate != null)
                    {
                        medicationInfo.CustomerMedicationInfo.ExpireDate = ipd.ExpireHealthInsuranceDate?.ToString(Constant.DATE_FORMAT);
                    }
                    else
                    {
                        medicationInfo.CustomerMedicationInfo.ExpireDate = "";
                    }

                    //Lấy thông tin chẩn đoán
                    medicationInfo.CustomerMedicationInfo.DiagnosisInfo = GetVisitDiagnosisAndICD(ipd.Id, "IPD", false);

                    if (medicationInfo.CustomerMedicationInfo.DiagnosisInfo == null || medicationInfo.CustomerMedicationInfo.DiagnosisInfo.Diagnosis == null || medicationInfo.CustomerMedicationInfo.DiagnosisInfo.Diagnosis == "")
                    {
                        medicationInfo.DiagnosisMessage = "Vui lòng bổ sung Chẩn đoán tại Bệnh án.";
                    }

                    //Lấy thông tin chiều cao, cân nặng, dị ứng
                    var ia = ipd.IPDInitialAssessmentForAdult;
                    if (ia != null && ia.IPDInitialAssessmentForAdultDatas != null && ia.IPDInitialAssessmentForAdultDatas.Count > 0)
                    {
                        medicationInfo.CustomerMedicationInfo.Height = ia.IPDInitialAssessmentForAdultDatas.FirstOrDefault(e => e.Code == "IPDIAAUHEIGANS")?.Value;
                        if (medicationInfo.CustomerMedicationInfo.Height == "" || medicationInfo.CustomerMedicationInfo.Height == null)
                        {
                            medicationInfo.HeightMessage = "Vui lòng bổ sung Chiều cao tại Đánh giá ban đầu người bệnh.";
                            medicationInfo.CustomerMedicationInfo.Height = "";
                        }

                        medicationInfo.CustomerMedicationInfo.Weight = ia.IPDInitialAssessmentForAdultDatas.FirstOrDefault(e => e.Code == "IPDIAAUWEIGANS")?.Value;
                        if (medicationInfo.CustomerMedicationInfo.Weight == "" || medicationInfo.CustomerMedicationInfo.Weight == null)
                        {
                            medicationInfo.WeightMessage = "Vui lòng bổ sung Cân nặng tại Đánh giá ban đầu người bệnh.";
                            medicationInfo.CustomerMedicationInfo.Weight = "";
                        }

                        //Dị ứng
                        if (ipd.IsAllergy == true)
                        {
                            medicationInfo.CustomerMedicationInfo.Allergy = GenerateAllergyString(ipd.KindOfAllergy, ipd.Allergy);
                        }
                        else
                        {
                            if (ipd.IPDInitialAssessmentForAdult != null)
                            {
                                if (ia != null && ia.IPDInitialAssessmentForAdultDatas != null && ia.IPDInitialAssessmentForAdultDatas.Count > 0)
                                {
                                    if (ia.IPDInitialAssessmentForAdultDatas.FirstOrDefault(e => e.Code == "IPDIAAUALLENOO")?.Value.ToUpper() == "TRUE")
                                    {
                                        medicationInfo.CustomerMedicationInfo.Allergy = "Không/ No";
                                    }
                                    else if (ia.IPDInitialAssessmentForAdultDatas.FirstOrDefault(e => e.Code == "IPDIAAUALLENPA")?.Value.ToUpper() == "TRUE")
                                    {
                                        medicationInfo.CustomerMedicationInfo.Allergy = "Không xác định/ N/A";
                                    }
                                    else
                                    {
                                        medicationInfo.CustomerMedicationInfo.Allergy = "";
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        medicationInfo.CustomerMedicationInfo.Allergy = "";
                        medicationInfo.HeightMessage = "Vui lòng bổ sung Chiều cao tại Đánh giá ban đầu người bệnh.";
                        medicationInfo.WeightMessage = "Vui lòng bổ sung Cân nặng tại Đánh giá ban đầu người bệnh.";
                    }
                }
                catch (Exception)
                {
                    medicationInfo.VisitCodeMessage = $"Vui lòng tiếp nhận lượt khám vào EFORM!";
                }
            }
            else
            {
                medicationInfo.VisitCodeMessage = $"Vui lòng tiếp nhận lượt khám vào EFORM!";
            }

            return medicationInfo;
        }

        private MedicationInfoModel GetEDMedicalInfo(string visitCode, string primaryDoctorUserName, string hospitalCode, Guid visitId)
        {
            MedicationInfoModel medicationInfo = new MedicationInfoModel();
            Guid userId = GetUserIdByUserName(primaryDoctorUserName);

            medicationInfo.HospitalInfo = GetHospitalInfo(hospitalCode);

            ED ed = new ED();
            if (visitId != Guid.Empty)
            {
                ed = GetED(visitId);
            }
            else
            {
                List<ED> listEDs = unitOfWork.EDRepository.Find(e => !e.IsDeleted && e.VisitCode.Contains(visitCode) && e.PrimaryDoctorId != null).ToList();
                if (listEDs != null && listEDs.Count > 0)
                {
                    ed = listEDs.FirstOrDefault(e => e.PrimaryDoctorId == userId);
                }
            }

            if (ed != null)
            {
                try
                {
                    if (ed.HealthInsuranceNumber != null)
                    {
                        medicationInfo.CustomerMedicationInfo.InsuranceCardNo = ed.HealthInsuranceNumber;
                    }
                    else
                    {
                        medicationInfo.CustomerMedicationInfo.InsuranceCardNo = "";
                    }

                    if (ed.ExpireHealthInsuranceDate != null)
                    {
                        medicationInfo.CustomerMedicationInfo.ExpireDate = ed.ExpireHealthInsuranceDate?.ToString(Constant.DATE_FORMAT);
                    }
                    else
                    {
                        medicationInfo.CustomerMedicationInfo.ExpireDate = "";
                    }

                    if (ed.Customer.DateOfBirth != null)
                    {
                        medicationInfo.CustomerMedicationInfo.DateOfBirth = Convert.ToDateTime(ed.Customer.DateOfBirth).ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        medicationInfo.CustomerMedicationInfo.DateOfBirth = "";
                    }

                    var etr = ed.EmergencyTriageRecord;
                    if (etr != null)
                    {

                        var datas = etr.EmergencyTriageRecordDatas;
                        if (datas != null && datas.Count > 0)
                        {
                            //Lấy chiều cao
                            medicationInfo.CustomerMedicationInfo.Height = datas.FirstOrDefault(e => e.Code == "ETRHEIANS")?.Value;
                            if (medicationInfo.CustomerMedicationInfo.Height == "" || medicationInfo.CustomerMedicationInfo.Height == null)
                            {
                                medicationInfo.HeightMessage = "Vui lòng bổ sung Chiều cao tại Phân loại cấp cứu.";
                            }
                            //Lấy cân nặng
                            //ETRWEIANS
                            medicationInfo.CustomerMedicationInfo.Weight = datas.FirstOrDefault(e => e.Code == "ETRWEIANS")?.Value;
                            if (medicationInfo.CustomerMedicationInfo.Weight == "" || medicationInfo.CustomerMedicationInfo.Weight == null)
                            {
                                medicationInfo.WeightMessage = "Vui lòng bổ sung Cân nặng tại Phân loại cấp cứu.";
                            }

                            //Lấy dị ứng
                            var diUngKhongXacDinh = datas.FirstOrDefault(e => e.Code == "ETRALLNPA")?.Value.ToUpper();
                            var diUngXacDinh = datas.FirstOrDefault(e => e.Code == "ETRALLYES")?.Value.ToUpper();
                            if (diUngKhongXacDinh == "TRUE")
                            {
                                medicationInfo.CustomerMedicationInfo.Allergy = "Không xác định/ N/A";
                            }
                            else if (diUngXacDinh == "TRUE")
                            {
                                var allergyType = datas.FirstOrDefault(e => e.Code == "ETRALLKOA")?.Value;
                                if (allergyType != null && allergyType.Contains(","))
                                {
                                    string[] allergyTypes = allergyType.Split(',');
                                    for (int i = 0; i < allergyTypes.Length; i++)
                                    {
                                        switch (allergyTypes[i])
                                        {
                                            case "1":
                                                medicationInfo.CustomerMedicationInfo.Allergy += "Thực phẩm/ Food, ";
                                                break;
                                            case "2":
                                                medicationInfo.CustomerMedicationInfo.Allergy += "Thời tiết/ Weather, ";
                                                break;
                                            case "3":
                                                medicationInfo.CustomerMedicationInfo.Allergy += "Thuốc/ Medicine, ";
                                                break;
                                            case "4":
                                                medicationInfo.CustomerMedicationInfo.Allergy += "Khác/ Other, ";
                                                break;
                                        }
                                    }
                                    medicationInfo.CustomerMedicationInfo.Allergy = medicationInfo.CustomerMedicationInfo.Allergy.Remove(medicationInfo.CustomerMedicationInfo.Allergy.LastIndexOf(","));
                                }
                                else if (allergyType != null && !allergyType.Contains(","))
                                {
                                    switch (allergyType)
                                    {
                                        case "1":
                                            medicationInfo.CustomerMedicationInfo.Allergy = "Thực phẩm/ Food";
                                            break;
                                        case "2":
                                            medicationInfo.CustomerMedicationInfo.Allergy = "Thời tiết/ Weather";
                                            break;
                                        case "3":
                                            medicationInfo.CustomerMedicationInfo.Allergy = "Thuốc/ Medicine";
                                            break;
                                        case "4":
                                            medicationInfo.CustomerMedicationInfo.Allergy = "Khác/ Other";
                                            break;
                                    }
                                }

                                var allergyContent = datas.FirstOrDefault(e => e.Code == "ETRALLANS")?.Value;

                                if (allergyContent != null || allergyContent != "")
                                {
                                    medicationInfo.CustomerMedicationInfo.Allergy += $" - {allergyContent}";
                                }

                                medicationInfo.CustomerMedicationInfo.DiagnosisInfo = GetVisitDiagnosisAndICD(ed.Id, "ED", true);
                                if (medicationInfo.CustomerMedicationInfo.DiagnosisInfo == null || medicationInfo.CustomerMedicationInfo.DiagnosisInfo.Diagnosis == null || medicationInfo.CustomerMedicationInfo.DiagnosisInfo.Diagnosis == "")
                                {
                                    medicationInfo.DiagnosisMessage = "Vui lòng bổ sung Chẩn đoán tại Bệnh án.";
                                }
                            }
                            else
                            {
                                medicationInfo.CustomerMedicationInfo.Allergy = "Không/ No";
                            }

                            //Lấy thông tin chẩn đoán
                            medicationInfo.CustomerMedicationInfo.DiagnosisInfo = GetVisitDiagnosisAndICD(ed.Id, "ED", true);
                            if (medicationInfo.CustomerMedicationInfo.DiagnosisInfo == null)
                            {
                                medicationInfo.DiagnosisMessage = "Vui lòng bổ sung Chẩn đoán tại hồ sơ bệnh nhân.";
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    medicationInfo.VisitCodeMessage = $"Vui lòng tiếp nhận lượt khám vào EFORM!";
                }
            }
            else
            {
                medicationInfo.VisitCodeMessage = $"Vui lòng tiếp nhận lượt khám vào EFORM!";
            }
            return medicationInfo;
        }

        private MedicationInfoModel GetOPDMedicalInfo(string visitCode, string primaryDoctorUserName, string hospitalCode, Guid visitId)
        {
            MedicationInfoModel medicationInfo = new MedicationInfoModel();
            Guid userId = GetUserIdByUserName(primaryDoctorUserName);

            medicationInfo.HospitalInfo = GetHospitalInfo(hospitalCode);

            OPD opd = new OPD();

            if (visitId != Guid.Empty)
            {
                opd = GetOPD(visitId);
            }
            else
            {
                List<OPD> listOpds = unitOfWork.OPDRepository.Find(e => !e.IsDeleted && e.VisitCode.Contains(visitCode) && e.PrimaryDoctorId != null).ToList();
                if (listOpds != null && listOpds.Count > 0)
                {
                    opd = listOpds.FirstOrDefault(e => e.PrimaryDoctorId == userId);
                }
            }

            if (opd != null)
            {
                try
                {
                    if (opd.HealthInsuranceNumber != null)
                    {
                        medicationInfo.CustomerMedicationInfo.InsuranceCardNo = opd.HealthInsuranceNumber;
                    }
                    else
                    {
                        medicationInfo.CustomerMedicationInfo.InsuranceCardNo = "";
                    }

                    if (opd.ExpireHealthInsuranceDate != null)
                    {
                        medicationInfo.CustomerMedicationInfo.ExpireDate = opd.ExpireHealthInsuranceDate?.ToString(Constant.DATE_FORMAT);
                    }
                    else
                    {
                        medicationInfo.CustomerMedicationInfo.ExpireDate = "";
                    }

                    if (opd.Customer.DateOfBirth != null)
                    {
                        medicationInfo.CustomerMedicationInfo.DateOfBirth = Convert.ToDateTime(opd.Customer.DateOfBirth).ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        medicationInfo.CustomerMedicationInfo.DateOfBirth = "";
                    }

                    var iafst = opd.OPDInitialAssessmentForShortTerm.OPDInitialAssessmentForShortTermDatas;
                    if (iafst != null && iafst.Count > 0)
                    {
                        medicationInfo.CustomerMedicationInfo.Height = iafst.FirstOrDefault(e => e.Code == "OPDIAFSTOPHEIANS")?.Value;
                        if (medicationInfo.CustomerMedicationInfo.Height == "" || medicationInfo.CustomerMedicationInfo.Height == null)
                        {
                            medicationInfo.HeightMessage = "Vui lòng bổ sung Chiều cao tại Đánh giá ban đầu người bệnh.";
                        }

                        medicationInfo.CustomerMedicationInfo.Weight = iafst.FirstOrDefault(e => e.Code == "OPDIAFSTOPWEIANS")?.Value;
                        if (medicationInfo.CustomerMedicationInfo.Weight == "" || medicationInfo.CustomerMedicationInfo.Weight == null)
                        {
                            medicationInfo.WeightMessage = "Vui lòng bổ sung Cân nặng tại Đánh giá ban đầu người bệnh.";
                        }

                        var diUngKhongXacDinh = iafst.FirstOrDefault(e => e.Code == "OPDIAFSTOPALLNPA")?.Value.ToUpper();
                        var diUngXacDinh = iafst.FirstOrDefault(e => e.Code == "OPDIAFSTOPALLYES")?.Value.ToUpper();

                        if (diUngKhongXacDinh == "TRUE")
                        {
                            medicationInfo.CustomerMedicationInfo.Allergy = "Không xác định/ N/A";
                        }
                        else if (diUngXacDinh == "TRUE")
                        {
                            var allergyType = iafst.FirstOrDefault(e => e.Code == "OPDIAFSTOPALLKOA")?.Value;
                            if (allergyType != null && allergyType.Contains(","))
                            {
                                string[] allergyTypes = allergyType.Split(',');
                                for (int i = 0; i < allergyTypes.Length; i++)
                                {
                                    switch (allergyTypes[i])
                                    {
                                        case "1":
                                            medicationInfo.CustomerMedicationInfo.Allergy += "Thực phẩm/ Food, ";
                                            break;
                                        case "2":
                                            medicationInfo.CustomerMedicationInfo.Allergy += "Thời tiết/ Weather, ";
                                            break;
                                        case "3":
                                            medicationInfo.CustomerMedicationInfo.Allergy += "Thuốc/ Medicine, ";
                                            break;
                                        case "4":
                                            medicationInfo.CustomerMedicationInfo.Allergy += "Khác/ Other, ";
                                            break;
                                    }
                                }

                                medicationInfo.CustomerMedicationInfo.Allergy = medicationInfo.CustomerMedicationInfo.Allergy.Remove(medicationInfo.CustomerMedicationInfo.Allergy.LastIndexOf(","));
                            }
                            else if (allergyType != null && !allergyType.Contains(","))
                            {
                                switch (allergyType)
                                {
                                    case "1":
                                        medicationInfo.CustomerMedicationInfo.Allergy = "Thực phẩm/ Food";
                                        break;
                                    case "2":
                                        medicationInfo.CustomerMedicationInfo.Allergy = "Thời tiết/ Weather";
                                        break;
                                    case "3":
                                        medicationInfo.CustomerMedicationInfo.Allergy = "Thuốc/ Medicine";
                                        break;
                                    case "4":
                                        medicationInfo.CustomerMedicationInfo.Allergy = "Khác/ Other";
                                        break;
                                }
                            }

                            //OPDIAFSTOPALLANS
                            var allergyContent = iafst.FirstOrDefault(e => e.Code == "OPDIAFSTOPALLANS")?.Value;


                            if (allergyContent != null || allergyContent != "")
                            {
                                medicationInfo.CustomerMedicationInfo.Allergy += $" - {allergyContent}";
                            }
                        }
                        else
                        {
                            medicationInfo.CustomerMedicationInfo.Allergy = "Không/ No";
                        }

                        //Lấy thông tin chẩn đoán
                        medicationInfo.CustomerMedicationInfo.DiagnosisInfo = GetVisitDiagnosisAndICD(opd.Id, "OPD", true);
                        if (medicationInfo.CustomerMedicationInfo.DiagnosisInfo == null)
                        {
                            medicationInfo.DiagnosisMessage = "Vui lòng bổ sung Chẩn đoán tại Phiếu khám ngoại trú.";
                        }
                    }
                }
                catch (Exception e)
                {
                    medicationInfo.VisitCodeMessage = $"Vui lòng tiếp nhận lượt khám vào EFORM!";
                }
            }
            else
            {
                medicationInfo.VisitCodeMessage = $"Vui lòng tiếp nhận lượt khám vào EFORM!";
            }

            return medicationInfo;
        }

        private Site GetHospitalInfo(string hospitalCode)
        {
            Site site = new Site();

            List<Site> sites = unitOfWork.SiteRepository.Find(e => !e.IsDeleted).ToList();
            if (sites != null && sites.Count > 0)
            {
                site = sites.FirstOrDefault(e => e.ApiCode == "HTC");
                if (site != null)
                {
                    site = sites.FirstOrDefault(e => e.ApiCode == "HTC");
                }

                site = sites.FirstOrDefault(e => e.ApiCode == hospitalCode);
            }
            return site;
        }
        private Guid GetUserIdByUserName(string userName)
        {
            User user = unitOfWork.UserRepository.FirstOrDefault(e => e.Username.ToLower() == userName.ToLower());
            if (user != null)
            {
                return user.Id;
            }
            else
            {
                return Guid.Empty;
            }
        }

        private string GenerateAllergyString(string kindOfAllergy, string allergy)
        {
            string diUng = "";
            if (kindOfAllergy.Contains(','))
            {
                string[] allergyTypes = kindOfAllergy.Split(',');
                for (int i = 0; i < allergyTypes.Length; i++)
                {
                    switch (allergyTypes[i])
                    {
                        case "1":
                            diUng += "Thực phẩm/ Food, ";
                            break;
                        case "2":
                            diUng += "Thời tiết/ Weather, ";
                            break;
                        case "3":
                            diUng += "Thuốc/ Medicine, ";
                            break;
                        case "4":
                            diUng += "Khác/ Other, ";
                            break;
                    }
                }

                diUng = diUng.Remove(diUng.LastIndexOf(','), 1);
            }
            else
            {
                switch (kindOfAllergy)
                {
                    case "1":
                        diUng = "Thực phẩm/ Food";
                        break;
                    case "2":
                        diUng = "Thời tiết/ Weather";
                        break;
                    case "3":
                        diUng = "Thuốc/ Medicine";
                        break;
                    case "4":
                        diUng = "Khác/ Other";
                        break;
                }
            }

            diUng += $" - {allergy}";
            return diUng;
        }

        private Int32 GetAge(DateTime dob)
        {
            var today = DateTime.Today;
            var a = (today.Year * 100 + today.Month) * 100 + today.Day;
            var b = (dob.Year * 100 + dob.Month) * 100 + dob.Day;

            return (a - b) / 100;
        }

        public static Age CalculateAge(DateTime startDate, DateTime endDate)
        {
            if (startDate.Date > endDate.Date)
            {
                //throw new ArgumentException("startDate cannot be higher then endDate", "startDate");
            }

            int years = endDate.Year - startDate.Year;
            int months = 0;
            int days = 0;

            // Check if the last year, was a full year.
            if (endDate < startDate.AddYears(years) && years != 0)
            {
                years--;
            }

            // Calculate the number of months.
            startDate = startDate.AddYears(years);

            if (startDate.Year == endDate.Year)
            {
                months = endDate.Month - startDate.Month;
            }
            else
            {
                months = (12 - startDate.Month) + endDate.Month;
            }

            // Check if last month was a complete month.
            if (endDate < startDate.AddMonths(months) && months != 0)
            {
                months--;
            }

            // Calculate the number of days.
            startDate = startDate.AddMonths(months);

            days = (endDate - startDate).Days;

            return new Age(years, months, days);
        }
    }
}