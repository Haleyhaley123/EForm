using Common;
using DataAccess.Models.GeneralModel;
using DataAccess.Repository;
using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace SyncManager.ScheduleJobs
{
    public class SendSMSUtilDischargedJob : IJob
    {
        protected IUnitOfWork unitOfWork = new EfUnitOfWork();
        private readonly string smsType = "SMSDISCHANGED";
        public void Execute(IJobExecutionContext context)
        {
            DoJob();
        }

        public void Execute()
        {
            
                DoJob();
            
        }

        private void DoJob()
        {
            try
            {
                CustomLogs.intervaljoblog.Info($"<SendSMSUtilDischargedJob> Start!");
                if (ConfigHelper.CF_SendNotiToMyVinmec_CS_is_off)
                {
                    CustomLogs.intervaljoblog.Info($"<SendSMSUtilDischargedJob> is Off!");
                    return;
                }

            

                string statusCodeInConfig = ConfigurationManager.AppSettings["myvinmec-sms-StatusCode"].ToString();
                var curent_date = DateTime.Now.AddMinutes(Convert.ToDouble(ConfigurationManager.AppSettings["myvinmec-sms-AboutMinute"].ToString()));
                var vi_Nationality = string.IsNullOrEmpty(ConfigurationManager.AppSettings["myvinmec-sms-ViNationality"].ToString()) ? new List<string>() {"VNM"} : ConfigurationManager.AppSettings["myvinmec-sms-ViNationality"].ToString().ToUpper().Split(',').ToList();
                if (string.IsNullOrWhiteSpace(statusCodeInConfig))
                {
                    CustomLogs.intervaljoblog.Info($"<SendSMSUtilDischargedJob> END with status config is null");
                    return;
                }
                var statusCodes = statusCodeInConfig.Split(',');
                var status_id = unitOfWork.EDStatusRepository.Find(e => statusCodes.Contains(e.Code)).ToList().Select(e => e.Id).ToList();

                var om = DateTime.Now.AddDays(-7);
            
                var opds = unitOfWork.OPDRepository.Find(x => status_id.Contains((Guid)x.EDStatusId) && !x.IsDeleted
                                                                                                && x.OPDOutpatientExaminationNoteId != null
                                                                                                && x.UpdatedAt >= curent_date
                                                                                                && x.UpdatedAt <= DateTime.Now
                                                                                                && x.CreatedAt > om
                                                                                                ).ToList();
                CustomLogs.intervaljoblog.Info($"<SendSMSUtilDischargedJob> OPD" + opds.Count().ToString());
                var opd_send = 0;
                foreach (var opd in opds)
                {
                    // CustomLog.intervaljoblog.Info($"<SendSMSUtilDischargedJob> OPD!" + opd.Id.ToString());
                    var mailSend = unitOfWork.SendMailNotificationRepository.FirstOrDefault(x => x.FormId == opd.Id && x.To == "MYVINMEC");
                    if (mailSend == null)
                    {
                        var opd_cus = opd.Customer;
                        var opd_oen = opd.OPDOutpatientExaminationNote;
                        if (opd_oen.ExaminationTime != null)
                        {
                            string lang = string.IsNullOrWhiteSpace(opd_cus.Nationality) || !vi_Nationality.Contains(opd_cus.Nationality.ToUpper()) ? "en" : "vi";
                            CustomLogs.intervaljoblog.Info($"<SendSMSUtilDischargedJob> OPD!" + opd.Id.ToString() + " " + opd_send);
                            BodyAPI body = new BodyAPI()
                            {
                                pid = opd_cus.PID,
                                customer_name = opd_cus.Fullname,
                                phone_number = opd_cus.Phone,
                                doctor_name = opd?.PrimaryDoctor?.Fullname,
                                visit_code = opd.VisitCode,
                                visit_type = "OPD",
                                lang = lang,
                                message_type = "OPD1",
                                speciality = opd?.Specialty?.ViName,
                                hospital_code = opd?.Site?.ApiCode,
                                send_time = DateTime.Now.ToString(),
                                completed_time = opd_oen.UpdatedAt.ToString(),
                                examination_date = opd_oen.ExaminationTime?.ToString(),
                                emr_visit_type = "OPD"
                            };
                            SendSMS(opd.Id, body);
                            opd_send++;
                        }
                    }
                }
                CustomLogs.intervaljoblog.Info($"<SendSMSUtilDischargedJob> OPD end with" + opd_send.ToString());

                var edcs = unitOfWork.EOCRepository.Find(x => status_id.Contains((Guid)x.StatusId) && !x.IsDeleted
                                                                                                && x.UpdatedAt >= curent_date
                                                                                                && x.UpdatedAt <= DateTime.Now
                                                                                                && x.CreatedAt > om
                                                                                                ).ToList();
                CustomLogs.intervaljoblog.Info($"<SendSMSUtilDischargedJob> EDC" + edcs.Count().ToString());
                var edc_send = 0;
                foreach (var edc in edcs)
                {
                    var mailSend = unitOfWork.SendMailNotificationRepository.FirstOrDefault(x => x.FormId == edc.Id && x.To == "MYVINMEC");
                    if (mailSend == null)
                    {
                        var examinationNote = unitOfWork.EOCOutpatientExaminationNoteRepository.Find(e => e.VisitId == edc.Id).FirstOrDefault();
                        if (examinationNote != null && examinationNote.ExaminationTime != null)
                        {
                            var eoc_cus = edc.Customer;
                            string lang = string.IsNullOrWhiteSpace(eoc_cus.Nationality) || !vi_Nationality.Contains(eoc_cus.Nationality.ToUpper()) ? "en" : "vi";
                            BodyAPI body = new BodyAPI()
                            {
                                pid = eoc_cus.PID,
                                customer_name = eoc_cus.Fullname,
                                phone_number = eoc_cus.Phone,
                                doctor_name = edc?.PrimaryDoctor?.Fullname,
                                visit_code = edc?.VisitCode,
                                visit_type = "EOD",
                                lang = lang,
                                message_type = "EOD1",
                                speciality = edc?.Specialty?.ViName,
                                hospital_code = edc?.Site?.ApiCode,
                                send_time = DateTime.Now.ToString(),
                                completed_time = examinationNote.UpdatedAt.ToString(),
                                examination_date = examinationNote.ExaminationTime?.ToString(),
                                emr_visit_type = "EDC"
                            };
                            SendSMS(edc.Id, body, "EDC");
                            edc_send++;
                        }
                    }
                }
                CustomLogs.intervaljoblog.Info($"<SendSMSUtilDischargedJob> EDC end with" + edc_send.ToString());
            
                var eds = unitOfWork.EDRepository.Find(x => status_id.Contains((Guid)x.EDStatusId) && !x.IsDeleted
                                                                                                && x.UpdatedAt >= curent_date
                                                                                                && x.UpdatedAt <= DateTime.Now
                                                                                                && x.CreatedAt > om
                                                                                                ).ToList();
                CustomLogs.intervaljoblog.Info($"<SendSMSUtilDischargedJob> ED" + eds.Count().ToString());
                var eds_send = 0;
                foreach (var edvisit in eds)
                {
                    var mailSend = unitOfWork.SendMailNotificationRepository.FirstOrDefault(x => x.FormId == edvisit.Id && x.To == "MYVINMEC");
                    //  || (mailSend.Status != "SENDED" && mailSend.Status != "SENDING" && mailSend.ErrorCount < 3)
                    if (mailSend == null)
                    {
                        var examinationNote = unitOfWork.DischargeInformationRepository.Find(e => e.Id == edvisit.DischargeInformationId).FirstOrDefault();
                    
                        if (examinationNote != null && edvisit.AdmittedDate != null)
                        {
                            var ed_cus = edvisit.Customer;
                            string lang = string.IsNullOrWhiteSpace(ed_cus.Nationality) || !vi_Nationality.Contains(ed_cus.Nationality.ToUpper()) ? "en" : "vi";
                            BodyAPI body = new BodyAPI()
                            {
                                pid = ed_cus.PID,
                                customer_name = ed_cus.Fullname,
                                phone_number = ed_cus.Phone,
                                doctor_name = edvisit?.PrimaryDoctor?.Fullname,
                                visit_code = edvisit?.VisitCode,
                                visit_type = "ED",
                                lang = lang,
                                message_type = "ED1",
                                speciality = edvisit?.Specialty?.ViName,
                                hospital_code = edvisit?.Site?.ApiCode,
                                send_time = DateTime.Now.ToString(),
                                completed_time = examinationNote.UpdatedAt.ToString(),
                                examination_date = edvisit.AdmittedDate.ToString(),
                                emr_visit_type = "ED"
                            };
                            SendSMS(edvisit.Id, body, "ED");
                            eds_send++;
                        }
                    }
                }
                CustomLogs.intervaljoblog.Info($"<SendSMSUtilDischargedJob> EDC end with" + eds_send.ToString());
                var ipds = unitOfWork.IPDRepository.Find(x => status_id.Contains((Guid)x.EDStatusId) && !x.IsDeleted
                                                                                                && x.UpdatedAt >= curent_date
                                                                                                && x.UpdatedAt <= DateTime.Now
                                                                                                && x.CreatedAt > om
                                                                                                ).ToList();
                CustomLogs.intervaljoblog.Info($"<SendSMSUtilDischargedJob> IPD" + ipds.Count().ToString());
                var ipds_send = 0;
                foreach (var ipdvisit in ipds)
                {
                    var mailSend = unitOfWork.SendMailNotificationRepository.FirstOrDefault(x => x.FormId == ipdvisit.Id && x.To == "MYVINMEC");
                    if (mailSend == null)
                    {
                        var examinationNote = unitOfWork.IPDMedicalRecordRepository.Find(e => e.Id == ipdvisit.IPDMedicalRecordId).FirstOrDefault();
                        if (examinationNote != null && ipdvisit.AdmittedDate != null)
                        {
                            var ipd_cus = ipdvisit.Customer;
                            string lang = string.IsNullOrWhiteSpace(ipd_cus.Nationality) || !vi_Nationality.Contains(ipd_cus.Nationality.ToUpper()) ? "en" : "vi";
                            BodyAPI body = new BodyAPI()
                            {
                                pid = ipd_cus.PID,
                                customer_name = ipd_cus.Fullname,
                                phone_number = ipd_cus.Phone,
                                doctor_name = ipdvisit?.PrimaryDoctor?.Fullname,
                                visit_code = ipdvisit?.VisitCode,
                                visit_type = "IPD",
                                lang = lang,
                                message_type = "IPD1",
                                speciality = ipdvisit?.Specialty?.ViName,
                                hospital_code = ipdvisit?.Site?.ApiCode,
                                send_time = DateTime.Now.ToString(),
                                completed_time = examinationNote.UpdatedAt.ToString(),
                                examination_date = ipdvisit.AdmittedDate.ToString(),
                                emr_visit_type = "IPD"
                            };
                            SendSMS(ipdvisit.Id, body, "IPD");
                            ipds_send++;
                        }
                    }
                }
                CustomLogs.intervaljoblog.Info($"<SendSMSUtilDischargedJob> IPD end with" + ipds_send.ToString());
                unitOfWork.Dispose();
                CustomLogs.intervaljoblog.Info($"<SendSMSUtilDischargedJob> END!");
            }
            catch (Exception ex)
            {
                CustomLogs.intervaljoblog.Info($"<SendSMSUtilDischargedJob> END with ERROR!" + ex.ToString());
                unitOfWork.Dispose();
            }
        }
        private SendMailNotification getOrCreate(Guid formId, string bodyCovert, string emr_visit_type = "OPD")
        {
            SendMailNotification noti = new SendMailNotification
            {
                ReceiverId = formId,
                FormId = formId,
                Type = smsType,
                Subject = emr_visit_type + ", Gửi sms khi Hoàn thành khám",
                Body = bodyCovert,
                To = "MYVINMEC",
                ErrorCount = 0,
                Status = "SENDING"
            };
            unitOfWork.SendMailNotificationRepository.Add(noti);
            unitOfWork.Commit();
            return noti;
        }
        private void SendSMS(Guid formId, BodyAPI body, string emr_visit_type = "OPD")
        {
            var bodyCovert = JsonConvert.SerializeObject(body);
            // CustomLog.intervaljoblog.Info($"<SendSMSUtilDischargedJob> SENDING!" + bodyCovert);
            var lastError = getOrCreate(formId, bodyCovert, emr_visit_type);
            try
            {
                var proxy_url = ConfigurationManager.AppSettings["ProxySever"];
                HttpClientHandler handler1 = new HttpClientHandler()
                {
                    UseProxy = true,
                    PreAuthenticate = true,
                    UseDefaultCredentials = true,
                };
                if (proxy_url != null)
                {
                    handler1.Proxy = new WebProxy
                    {
                        Address = new Uri(ConfigurationManager.AppSettings["ProxySever"]),
                    };
                }

                handler1.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler1.DefaultProxyCredentials = CredentialCache.DefaultCredentials;
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                var url = ConfigurationManager.AppSettings["myvinmec-sms-url"].ToString();
                var client_id = ConfigurationManager.AppSettings["myvinmec-sms-client-id"].ToString();
                var client_secret = ConfigurationManager.AppSettings["myvinmec-sms-client-secret"].ToString();


                var client = new HttpClient(handler1);

                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla / 5.0");
                client.Timeout = TimeSpan.FromSeconds(20);
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(url),
                    Headers = {
                                { HttpRequestHeader.ContentType.ToString(), "application/json" },
                                { "Client-Id", client_id},
                                { "Client-Secret", client_secret},
                                { HttpRequestHeader.UserAgent.ToString(), "Mozilla / 5.0(Windows NT 10.0; Win64; x64) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 94.0.4606.81 Safari / 537.36"}
                              },
                    Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
                };

                CustomLogs.intervaljoblog.Info($"<SendSMSUtilDischargedJob> SENDING! " + formId.ToString());
                var response = client.SendAsync(request).Result;
                CustomLogs.intervaljoblog.Info($"<SendSMSUtilDischargedJob> SENDED! " + response.StatusCode.ToString());

                // CustomLog.intervaljoblog.Info($"<SendSMSUtilDischargedJob> SENDED!" + response.Content.ReadAsStringAsync().Result);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    lastError.Status = "SENDED";
                    unitOfWork.SendMailNotificationRepository.Update(lastError);
                }
                else
                {
                    lastError.ErrorCount++;
                    lastError.Status = "ERROR";
                    lastError.ErrorMessenge += "{" + DateTime.Now.ToString() + " - StatusCode:" + (int)response.StatusCode + " - " + response.Content.ReadAsStringAsync().Result + "} ";
                    unitOfWork.SendMailNotificationRepository.Update(lastError);
                }
                unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                lastError.ErrorCount++;
                lastError.Status = "ERROR";
                lastError.ErrorMessenge += "{" + DateTime.Now.ToString() + " - Exception:" + ex.ToString();
                unitOfWork.SendMailNotificationRepository.Update(lastError);
                unitOfWork.Commit();
                CustomLogs.intervaljoblog.Info($"<SendSMSUtilDischargedJob> SENDED With ERROR! " + ex.ToString());
            }
        }
    }
    public class BodyAPI
    {
        public string pid { get; set; }
        public string customer_name { get; set; }
        public string phone_number { get; set; }
        public string doctor_name { get; set; }
        public string visit_code { get; set; }
        public string hospital_code { get; set; }
        private string _completedtime { get; set; }
        public string completed_time
        {
            get
            {
                return _completedtime;
            }
            set
            {
                _completedtime = DateTime.ParseExact(value, "M/d/yyyy h:mm:ss tt", null).ToString("yyyy-MM-ddTHH:mm:ss.fff+07:00");
            }
        }

        private string _examination;
        public string examination_date
        {
            get { return _examination; }
            set
            {
                _examination = DateTime.ParseExact(value, "M/d/yyyy h:mm:ss tt", null).ToString("yyyy-MM-ddTHH:mm:ss.fff+07:00");
            }
        }
        public string speciality { get; set; }
        private string _sendtime;
        public string send_time
        {
            get { return _sendtime; }
            set
            {
                _sendtime = DateTime.ParseExact(value, "M/d/yyyy h:mm:ss tt", null).ToString("yyyy-MM-ddTHH:mm:ss.fff+07:00");
            }
        }
        public string message_type { get; set; }
        public string visit_type { get; set; }
        public string lang { get; set; }
        public string emr_visit_type { get; set; }
    }
}