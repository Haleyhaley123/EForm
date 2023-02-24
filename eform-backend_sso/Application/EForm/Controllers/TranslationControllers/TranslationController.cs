using DataAccess.Models;
using EForm.Authentication;
using EForm.BaseControllers;
using EForm.Common;
using EForm.Models;
using EForm.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace EForm.Controllers.TranslationControllers
{
    [SessionAuthorize]
    public class TranslationController : BaseApiController
    {
        [HttpGet]
        [Route("api/Translation")]
        [Permission(Code = "TTRAN1")]
        public IHttpActionResult GetTranslationsAPI([FromUri] TranslateParameterModel request)
        {
            Nullable<DateTime> fromDate = new DateTime();
            Nullable<DateTime> toDate = new DateTime();

            if (request.FromDate != null && request.FromDate != "Invalid date")
            {
                fromDate = DateTime.ParseExact(request.FromDate.ToString(), "HH:mm dd/MM/yyyy", new CultureInfo("en-US"));
            }
            else
            {
                fromDate = null;
            }

            if (request.ToDate != null && request.ToDate != "Invalid date")
            {
                toDate = DateTime.ParseExact(request.ToDate.ToString(), "HH:mm dd/MM/yyyy", new CultureInfo("en-US"));
            }
            else
            {
                toDate = null;
            }

            var site_id = GetSiteId();
            var translation_query = unitOfWork.TranslationRepository.AsQueryable().Where(
                                                                                            e => !e.IsDeleted &&
                                                                                            e.CustomerId != null &&
                                                                                            e.SiteId != null && e.SiteId == site_id
                                                                                         )
                                                                                   .Join(unitOfWork.SpecialtyRepository.AsQueryable(), trans => trans.SpecialtyId, spec => spec.Id, (trans, spec) => new QueryTranslateModel
                                                                                   {
                                                                                       Id = trans.Id,
                                                                                       PID = trans.PID,
                                                                                       VisitCode = trans.VisitCode,
                                                                                       VisitTypeGroupCode = trans.VisitTypeGroupCode,
                                                                                       CustomerName = trans.CustomerName,
                                                                                       TranslationName = new { trans.ViName, trans.EnName },
                                                                                       RequestTime = trans.CreatedAt,
                                                                                       Status = trans.Status,
                                                                                       RequestedUsername = trans.CreatedBy,
                                                                                       TranslatedUsername = trans.TranslatedBy,
                                                                                       SpecialtyName = spec.ViName,
                                                                                       DateOfBirth = trans.Customer.DateOfBirth,
                                                                                       Phone = trans.Customer.Phone
                                                                                   }).AsQueryable();
                                    
            if (!string.IsNullOrEmpty(request.Search))
                translation_query = translation_query.Where(
                    e => (!string.IsNullOrEmpty(e.PID) && e.PID.Contains(request.Search)) ||
                    (!string.IsNullOrEmpty(e.CustomerName) && e.CustomerName.Contains(request.Search)) ||
                    (!string.IsNullOrEmpty(e.VisitCode) && e.VisitCode.Contains(request.Search))
                );

            var queryTranslateModels = translation_query.OrderBy(e => e.Status).ThenByDescending(e => e.RequestTime);
            var x_results = queryTranslateModels.ToList();
            List<string> listStatus = new List<string>();
            if (request.Status != null)
            {
                listStatus = request.Status.Split(',').ToList();
            }

            List<QueryTranslateModel> listFilterByStatus = new List<QueryTranslateModel>();
            if (listStatus.Count > 0)
            {
                listFilterByStatus = x_results.Where(e => listStatus.Contains(e.Status.ToString())).ToList();
            }
            else
            {
                listFilterByStatus = x_results;
            }

            List<QueryTranslateModel> listFilterByFromDate = new List<QueryTranslateModel>();

            if (fromDate != null)
            {
                listFilterByFromDate = listFilterByStatus.Where(e => (DateTime)e.RequestTime >= fromDate).ToList();
            }
            else
            {
                listFilterByFromDate = listFilterByStatus;
            }

            List<QueryTranslateModel> listFilterByToDate = new List<QueryTranslateModel>();
            if (toDate != null)
            {
                DateTime td = (DateTime)toDate;
                listFilterByToDate = listFilterByFromDate.Where(e => (DateTime)e.RequestTime <= td.AddMinutes(1)).ToList();
            }
            else
            {
                listFilterByToDate = listFilterByFromDate;
            }

            List<QueryTranslateModel> listFilterByUserName = new List<QueryTranslateModel>();
            if (request.User != null)
            {
                List<string> listUserIds = new List<string>();
                if (request.User.Contains(','))
                {
                    listUserIds = request.User.Split(',').ToList();
                }
                else
                {
                    listUserIds.Add(request.User);
                }

                if (listUserIds.Count == 1)
                {
                    Guid userId = Guid.Parse(request.User);
                    User user = unitOfWork.UserRepository.FirstOrDefault(e => e.Id == userId);
                    listFilterByUserName = listFilterByToDate.Where(e => e.RequestedUsername.ToUpper() == user.Username.ToUpper() || (e.TranslatedUsername != null && e.TranslatedUsername.ToUpper() == user.Username.ToUpper())).ToList();
                }
                else if (listUserIds.Count > 1)
                {
                    List<Guid> userIds = new List<Guid>();
                    for (int i = 0; i < listUserIds.Count; i++)
                    {
                        userIds.Add(Guid.Parse(listUserIds[i]));
                    }

                    List<User> users = unitOfWork.UserRepository.Find(e => userIds.Contains(e.Id)).ToList();

                    foreach (User user in users)
                    {
                        List<QueryTranslateModel> listItems = listFilterByToDate.Where(e => e.RequestedUsername.ToUpper() == user.Username.ToUpper() || (e.TranslatedUsername != null && e.TranslatedUsername.ToUpper() == user.Username.ToUpper())).ToList();
                        listFilterByUserName.AddRange(listItems);
                    }

                }

            }
            else
            {
                listFilterByUserName = listFilterByToDate;
            }

            int count = listFilterByUserName.Count;
            var y_results = listFilterByUserName.Select(e => new TranslateModel
            {
                Id = e.Id,
                PID = e.PID,
                VisitCode = e.VisitCode,
                VisitTypeGroupCode = e.VisitTypeGroupCode,
                CustomerName = e.CustomerName,
                TranslationName = e.TranslationName,
                RequestTime = e.RequestTime?.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND),
                Status = Constant.TRANSLATION_STATUS[e.Status],
                RequestedBy = e.RequestedUsername,
                TranslatedBy = e.TranslatedUsername,
                SpecialtyName = e.SpecialtyName,
                DateOfBirth = e.DateOfBirth,
                Phone = e.Phone
            }).Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize);


            return Content(HttpStatusCode.OK, new { count, results = y_results });
        }

        [HttpPost]
        [CSRFCheck]
        [Route("api/Translation/TranslationRequest")]
        [Permission(Code = "TTRAN2")]
        public IHttpActionResult TranslationRequestAPI([FromBody] TranslateRequest request)
        {

            dynamic visit = null;
            //if (request.VisitTypeGroupCode.Equals("ED"))
            //    visit = unitOfWork.EDRepository.GetById(request.VisitId);
            //else if (request.VisitTypeGroupCode.Equals("OPD"))
            //    visit = unitOfWork.OPDRepository.GetById(request.VisitId);
            //else if (request.VisitTypeGroupCode.Equals("IPD"))
            //    visit = unitOfWork.IPDRepository.GetById(request.VisitId);
            //else if (request.VisitTypeGroupCode.Equals("EOC"))
            //    visit = GetEOC(request.VisitId);
            visit = GetVisit(request.VisitId, request.VisitTypeGroupCode);
            if (visit == null)
                return Content(HttpStatusCode.NotFound, Message.VISIT_NOT_FOUND);

            if (string.IsNullOrEmpty(request.ToLanguage) || string.IsNullOrEmpty(request.FromLanguage))
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);

            if (HasTranslation(visit.Id, request.VisitTypeGroupCode, request.EnName, request.FromLanguage, request.ToLanguage))
                return Content(HttpStatusCode.BadRequest, Message.TRANSLATION_EXIST);

            var customer = visit.Customer;

            Translation translation = new Translation()
            {
                EnName = request?.EnName,
                ViName = request?.ViName,
                Status = 0,
                FromLanguage = request?.FromLanguage,
                ToLanguage = request?.ToLanguage,
                CustomerId = visit?.CustomerId,
                CustomerName = customer?.Fullname,
                PID = customer?.PID,
                VisitId = visit?.Id,
                VisitCode = visit?.VisitCode,
                VisitTypeGroupCode = request?.VisitTypeGroupCode,
                SiteId = visit.Specialty?.SiteId,
                SpecialtyId = visit.Specialty?.Id,
            };

            unitOfWork.TranslationRepository.Add(translation);
            unitOfWork.Commit();

            Guid positionId = unitOfWork.PositionRepository.FirstOrDefault(e => !e.IsDeleted && e.EnName == "Medical Secretary").Id;
            List<PositionUser> positionUsers = new List<PositionUser>();
            if (positionId != null)
            {
                positionUsers = unitOfWork.PositionUserRepository.Find(e => !e.IsDeleted && e.PositionId == positionId).ToList(); // Lấy tất cả userid là Thư kí chuyên môn
            }
            
            List<User> users = new List<User>();
            if (positionUsers.Count > 0)
            {
                foreach (var item in positionUsers)
                {
                    User tkcm = new User();
                    tkcm = unitOfWork.UserRepository.FirstOrDefault(e => !e.IsDeleted && e.Id == item.UserId);
                    if (tkcm != null)
                    {
                        users.Add(tkcm);
                    }
                }
            }

            var user = GetUser();
            var spec = visit.Specialty;

            List<User> medicalSecretaryOfSite = new List<User>();
            if (spec.Site != null)
            {
                try
                {
                    medicalSecretaryOfSite = users.Where(e => !e.IsDeleted && e.CurrentSiteId == spec.SiteId).ToList();
                }
                catch
                {

                }
            }

            if (medicalSecretaryOfSite.Count > 0)
            {
                foreach (var item in medicalSecretaryOfSite)
                {
                    var noti_creator = new NotificationCreator(
                        unitOfWork: unitOfWork,
                        from_user: user?.Username,
                        to_user: item.Username,
                        priority: 7,
                        vi_message: $"<b>[{request.VisitTypeGroupCode}-{spec?.ViName}]</b> Bạn có yêu cầu dịch <b>{request.ViName}</b> cho NB <b>{customer.PID} {customer.Fullname}</b> từ BS {user.Username}",
                        en_message: $"<b>[{request.VisitTypeGroupCode}-{spec?.ViName}]</b> Bạn có yêu cầu dịch <b>{request.ViName}</b> cho NB <b>{customer.PID} {customer.Fullname}</b> từ BS {user.Username}",
                        spec_id: spec?.Id,
                        visit_id: visit.Id,
                        group_code: "TKCM",
                        form_frontend: translation.Id.ToString()
                    );
                    noti_creator.Create();
                }
            }


            return Content(HttpStatusCode.OK, Message.SUCCESS);
        }

        [HttpPost]
        [CSRFCheck]
        [Route("api/Translation/Cancel/{id}")]
        [Permission(Code = "TTRAN2")]
        public IHttpActionResult Cancel(Guid id)
        {
            var translation = unitOfWork.TranslationRepository.GetById(id);
            if (translation == null)
                return Content(HttpStatusCode.NotFound, Message.TRANSLATION_NOT_FOUND);
            var user = GetUser();
            if (translation.CreatedBy != user.Username)
                return Content(HttpStatusCode.BadRequest, Message.OWNER_FORBIDDEN);

            List<Notification> needToDelete = unitOfWork.NotificationRepository.Find(e => e.Form == id.ToString()).ToList();

            if (needToDelete.Count > 0)
            {
                foreach (Notification item in needToDelete)
                {
                    item.IsDeleted = true;
                    unitOfWork.NotificationRepository.Update(item);
                    unitOfWork.Commit();
                }
            }

            unitOfWork.TranslationRepository.Delete(translation);


            unitOfWork.Commit();
            return Content(HttpStatusCode.OK, Message.SUCCESS);
        }
        [HttpPost]
        [CSRFCheck]
        [Route("api/Translation/Update/{id}")]
        [Permission(Code = "TTRAN3")]
        public IHttpActionResult UpdateTranslationDetailAPI(Guid id, [FromBody] JObject request)
        {
            var translation = unitOfWork.TranslationRepository.GetById(id);
            if (translation == null)
                return Content(HttpStatusCode.NotFound, Message.TRANSLATION_NOT_FOUND);

            var user = GetUser();
            var positions = user.PositionUsers.Where(e => !e.IsDeleted).Select(e => e.Position.EnName);
            if (positions.Contains("Medical Secretary"))
            {
                if (translation.Status > 1)
                    return Content(HttpStatusCode.BadRequest, Message.OWNER_FORBIDDEN);
                HandleUpdateOrCreateTranslationDatas(translation, request["Datas"], is_doctor: false);
                return Content(HttpStatusCode.OK, Message.SUCCESS);
            }
            else if (positions.Contains("Doctor"))
            {
                if (translation.Status != 2)
                    return Content(HttpStatusCode.BadRequest, Message.OWNER_FORBIDDEN);
                HandleUpdateOrCreateTranslationDatas(translation, request["Datas"], is_doctor: true);
                return Content(HttpStatusCode.OK, Message.SUCCESS);
            }
            return Content(HttpStatusCode.BadRequest, Message.OWNER_FORBIDDEN);
        }

        [HttpPost]
        [CSRFCheck]
        [Route("api/Translation/RequestConfirm/{id}")]
        [Permission(Code = "TTRAN4")]
        public IHttpActionResult RequestConfirmTranslationAPI(Guid id)
        {
            var translation = unitOfWork.TranslationRepository.GetById(id);
            if (translation == null)
                return Content(HttpStatusCode.NotFound, Message.TRANSLATION_NOT_FOUND);

            if (translation.Status > 1)
                return Content(HttpStatusCode.BadRequest, Message.OWNER_FORBIDDEN);

            translation.Status = 2;
            unitOfWork.TranslationRepository.Update(translation);
            unitOfWork.Commit();

            PushRequestConfirmNotification(translation);

            return Content(HttpStatusCode.OK, Message.SUCCESS);
        }

        [HttpPost]
        [CSRFCheck]
        [Route("api/Translation/RequestChange/{id}")]
        [Permission(Code = "TTRAN5")]
        public IHttpActionResult RequestConfirmTranslationAPI(Guid id, [FromBody] JObject request)
        {
            var translation = unitOfWork.TranslationRepository.GetById(id);
            if (translation == null)
                return Content(HttpStatusCode.NotFound, Message.TRANSLATION_NOT_FOUND);

            if (translation.Status == 0)
                return Content(HttpStatusCode.BadRequest, Message.TRANSLATION_IS_WAITING_TRANSLATE);

            var user = GetUser();
            var positions = user.PositionUsers.Where(e => !e.IsDeleted).Select(e => e.Position.EnName);
            if (!positions.Contains("Doctor"))
                return Content(HttpStatusCode.BadRequest, Message.OWNER_FORBIDDEN);
            if (!user.Username.Equals(translation.CreatedBy) && translation.VisitTypeGroupCode == "OPD")
                return Content(HttpStatusCode.BadRequest, Message.OWNER_FORBIDDEN);

            translation.Status = 1;
            translation.Note = request["Note"]?.ToString();
            unitOfWork.TranslationRepository.Update(translation);
            unitOfWork.Commit();

            PushRequestChangeNotification(translation);

            return Content(HttpStatusCode.OK, Message.SUCCESS);
        }

        [HttpPost]
        [CSRFCheck]
        [Route("api/Translation/Accept/{id}")]
        [Permission(Code = "TTRAN6")]
        public IHttpActionResult ConfirmTranslationAPI(Guid id)
        {
            var translation = unitOfWork.TranslationRepository.GetById(id);
            if (translation == null)
                return Content(HttpStatusCode.NotFound, Message.TRANSLATION_NOT_FOUND);

            if (translation.Status == 0)
                return Content(HttpStatusCode.BadRequest, Message.TRANSLATION_IS_WAITING_TRANSLATE);
            else if (translation.Status == 3)
                return Content(HttpStatusCode.BadRequest, Message.TRANSLATION_IS_CONFIRMED);

            var user = GetUser();
            var positions = user.PositionUsers.Where(e => !e.IsDeleted).Select(e => e.Position.EnName);
            if (!positions.Contains("Doctor"))
                return Content(HttpStatusCode.BadRequest, Message.OWNER_FORBIDDEN);

            if (!user.Username.Equals(translation.CreatedBy))
            {
                if (!IsCorectDoctorForOPD(user, translation.VisitTypeGroupCode, translation.VisitId))
                    return Content(HttpStatusCode.BadRequest, Message.OWNER_FORBIDDEN);
                PushConfirmNotificationForDoctor(translation);
            }

            translation.Status = 3;
            unitOfWork.TranslationRepository.Update(translation);
            unitOfWork.Commit();

            PushConfirmNotificationForSecretary(translation);

            return Content(HttpStatusCode.OK, Message.SUCCESS);
        }
        private bool IsCorectDoctorForOPD(User user, string visitTypeGroupCode, Guid? visitId)
        {
            if (visitTypeGroupCode == "OPD" || visitTypeGroupCode == "EOC")
            {
                var visit = GetVisit((Guid)visitId, visitTypeGroupCode);
                return (((visit.PrimaryDoctorId != null && visit.PrimaryDoctorId == user.Id) || (visit.AuthorizedDoctorId != null && visit.AuthorizedDoctorId == user.Id)));
            }
            return true;
        }
        private bool IsDoctorForOPD(User user, string visitTypeGroupCode, Guid? visitId)
        {
            if (visitTypeGroupCode == "OPD" || visitTypeGroupCode == "EOC")
            {
                var visit = GetVisit((Guid)visitId, visitTypeGroupCode);
                return (((visit.PrimaryDoctorId != null && visit.PrimaryDoctorId == user.Id) || (visit.AuthorizedDoctorId != null && visit.AuthorizedDoctorId == user.Id)));
            }
            return false;
        }

        private void HandleUpdateOrCreateTranslationDatas(Translation trans, JToken trans_requests, bool is_doctor)
        {
            var trans_datas = trans.TranslationDatas.Where(e => !e.IsDeleted).ToList();
            var user = GetUser();
            var form = "TRANSLATE" + trans.FromLanguage;
            var transdataDb = unitOfWork.TranslationRepository.Find(x => x.VisitId == trans.VisitId && !x.IsDeleted && x.Status != 3 && x.ToLanguage == trans.ToLanguage).Join(unitOfWork.TranslationDataRepository.AsQueryable(), t => t.Id, tdata => tdata.TranslationId, (t, tdata) => new TranslationData
            {
                Id = tdata.Id,
                Code = tdata.Code,
                Value = tdata.Value
                
            }).ToList();
            foreach (var item in trans_requests)
            {
                string code = item.Value<string>("Code");
                if (string.IsNullOrEmpty(item.Value<string>("Code"))) continue;

                string value = item.Value<string>("Value");
                var tran_data = GetOrCreateTranslationData(trans_datas, trans.Id, code, value);
                if (tran_data != null)
                    UpdateTranslationData(tran_data, value);
                if (!is_doctor)
                {
                    trans.TranslatedBy = GetUser().Username;
                    unitOfWork.TranslationRepository.Update(trans);
                }
                //if(trans.Status != 3)
                //{
                    var defaultValue = unitOfWork.MasterDataRepository.FirstOrDefault(x => x.Code == code && !x.IsDeleted)?.DefaultValue;
                    if (!string.IsNullOrEmpty(defaultValue))
                    {
                        var codes = unitOfWork.MasterDataRepository.Find(x => x.DefaultValue == defaultValue && !x.IsDeleted && x.Form == form).ToList();
                        foreach (var cd in codes)
                        {
                            var data = transdataDb.AsQueryable().Where(x => x.Code == cd.Code).ToList();
                            foreach(var d in data)
                            {
                                var t = unitOfWork.TranslationDataRepository.FirstOrDefault(x => x.Id == d.Id);
                                t.Value = value;
                                unitOfWork.TranslationDataRepository.Update(t);
                            }
                        }
                    }
                //}
            }
            unitOfWork.Commit();
        }
        private TranslationData GetOrCreateTranslationData(List<TranslationData> trans_datas, Guid trans_id, string code, string value)
        {
            TranslationData data = trans_datas.FirstOrDefault(
                e => !string.IsNullOrEmpty(e.Code) &&
                e.Code.Equals(code)
            );
            if (data == null)
            {
                data = new TranslationData()
                {
                    TranslationId = trans_id,
                    Code = code,
                    Value = value,
                };
                unitOfWork.TranslationDataRepository.Add(data);
            }
            return data;
        }
        private void UpdateTranslationData(TranslationData data, string value)
        {
            data.Value = value;
            unitOfWork.TranslationDataRepository.Update(data);
        }
        private bool IsTranslator(string username, string translator)
        {
            if (string.IsNullOrEmpty(username)) return false;
            if (string.IsNullOrEmpty(translator)) return true;
            if (!string.IsNullOrEmpty(translator) && username.Equals(translator)) return true;
            return false;
        }


        private Specialty GetSpecialtyByVisit(Guid visit_id, string group_code)
        {
            dynamic visit = null;
            if (group_code.Equals("ED"))
                visit = unitOfWork.EDRepository.GetById(visit_id);
            else if (group_code.Equals("OPD"))
                visit = unitOfWork.OPDRepository.GetById(visit_id);
            else if (group_code.Equals("IPD"))
                visit = unitOfWork.IPDRepository.GetById(visit_id);
            else if (group_code.Equals("EOC"))
                visit = unitOfWork.EOCRepository.GetById(visit_id);
            if (visit != null)
            {
                var spec = unitOfWork.SpecialtyRepository.GetById(visit.SpecialtyId);
                return spec;
            }
            return null;
        }
        private Specialty GetMedicalSecretarySpecialtyBySiteId(Guid site_id)
        {
            return unitOfWork.SpecialtyRepository.FirstOrDefault(
                e => !e.IsDeleted &&
                e.SiteId != null &&
                e.SiteId == site_id &&
                e.VisitTypeGroupId != null &&
                e.VisitTypeGroup.Code == "TKCM"
            );
        }
        private void PushRequestConfirmNotification(Translation translation)
        {
            var user = GetUser();
            var spec = GetSpecialtyByVisit((Guid)translation.VisitId, translation.VisitTypeGroupCode);
            string vi_mes = string.Format(
                    "<b>[{0}- {1}]</b> Bạn nhận được yêu cầu phê duyệt {4} của bệnh nhân <b>{2}</b> từ TKCM <b>{3}</b>",
                    translation.VisitTypeGroupCode,
                    spec?.ViName,
                    translation.CustomerName,
                    user?.Fullname,
                    translation.ViName
                );
            string en_mes = string.Format(
                    "<b>[{0}- {1}]</b> Bạn nhận được yêu cầu phê duyệt {4} của bệnh nhân <b>{2}</b> từ TKCM <b>{3}</b>",
                    translation.VisitTypeGroupCode,
                    spec?.ViName,
                    translation.CustomerName,
                    user?.Fullname,
                     translation.ViName
                );
            string form_frontend = string.Format("{0}MedicalReport", translation.VisitTypeGroupCode);
            if (!string.IsNullOrEmpty(translation.VisitTypeGroupCode) && translation.VisitTypeGroupCode == "IPD")
                form_frontend = "IPDDischargeMedicalReport";

            var noti_creator = new NotificationCreator(
                unitOfWork: unitOfWork,
                from_user: user?.Username,
                to_user: translation.CreatedBy,
                priority: 5,
                vi_message: vi_mes,
                en_message: en_mes,
                spec_id: spec.Id,
                visit_id: (Guid)translation.VisitId,
                group_code: translation.VisitTypeGroupCode,
                form_frontend: form_frontend
            );
            noti_creator.Create();
        }
        private void PushRequestChangeNotification(Translation translation)
        {
            var user = GetUser();
            var spec = GetMedicalSecretarySpecialtyBySiteId((Guid)translation.SiteId);
            var noti_creator = new NotificationCreator(
                unitOfWork: unitOfWork,
                from_user: user?.Username,
                to_user: translation.TranslatedBy,
                priority: 1,
                vi_message: $"Bạn nhận được yêu cầu chỉnh sửa bản dịch <b>{translation.ViName}</b> của bệnh nhân <b>{translation.CustomerName}</b>",
                en_message: $"Bạn nhận được yêu cầu chỉnh sửa bản dịch <b>{translation.ViName}</b> của bệnh nhân <b>{translation.CustomerName}</b>",
                spec_id: spec.Id,
                visit_id: translation.Id,
                group_code: "TKCM",
                form_frontend: "TKCM"
            );
            noti_creator.Create();
        }
        private void PushConfirmNotificationForSecretary(Translation translation)
        {
            var user = GetUser();
            var spec = GetMedicalSecretarySpecialtyBySiteId((Guid)translation.SiteId);
            var noti_creator = new NotificationCreator(
                unitOfWork: unitOfWork,
                from_user: user?.Username,
                to_user: translation.TranslatedBy,
                priority: 1,
                vi_message: $"Bản dịch <b>{translation.ViName}</b> của bệnh nhân <b>{translation.CustomerName}</b> đã được <b>duyệt</b>",
                en_message: $"Bản dịch <b>{translation.ViName}</b> của bệnh nhân <b>{translation.CustomerName}</b> đã được <b>duyệt</b>",
                spec_id: spec.Id,
                visit_id: translation.Id,
                group_code: "TKCM",
                form_frontend: "TKCM"
            );
            noti_creator.Create();
        }
        private void PushConfirmNotificationForDoctor(Translation translation)
        {
            var user = GetUser();
            var spec = GetSpecialtyByVisit((Guid)translation.VisitId, translation.VisitTypeGroupCode);
            string form_frontend = string.Format("{0}MedicalReport", translation.VisitTypeGroupCode);
            if (!string.IsNullOrEmpty(translation.VisitTypeGroupCode) && translation.VisitTypeGroupCode == "IPD")
                form_frontend = "IPDDischargeMedicalReport";
            var noti_creator = new NotificationCreator(
                unitOfWork: unitOfWork,
                from_user: user?.Username,
                to_user: translation.CreatedBy,
                priority: 6,
                vi_message: $"<b>[{translation.VisitTypeGroupCode}-{spec?.ViName}]</b> Bác sĩ <b>{user.Fullname}</b> ( {user.Title} )  đã duyệt bản dịch <b>{translation.ViName}</b> của bệnh nhân <b>{translation.CustomerName}</b>",
                en_message: $"<b>[{translation.VisitTypeGroupCode}-{spec?.ViName}]</b> Bác sĩ <b>{user.Fullname}</b> ( {user.Title} )  đã duyệt bản dịch <b>{translation.ViName}</b> của bệnh nhân <b>{translation.CustomerName}</b>",
                spec_id: spec.Id,
                visit_id: (Guid)translation.VisitId,
                group_code: translation.VisitTypeGroupCode,
                form_frontend: form_frontend
            );
            noti_creator.Create();
        }

        private bool HasTranslation(Guid? visit_id, string visit_type_group_code, string en_name, string from_language, string to_language)
        {
            var translation = unitOfWork.TranslationRepository.FirstOrDefault(
                e => !e.IsDeleted &&
                e.VisitId != null &&
                e.VisitId == visit_id &&
                !string.IsNullOrEmpty(e.VisitTypeGroupCode) &&
                e.VisitTypeGroupCode.Equals(visit_type_group_code) &&
                !string.IsNullOrEmpty(e.EnName) &&
                e.EnName.Equals(en_name) &&
                !string.IsNullOrEmpty(e.ToLanguage) &&
                e.ToLanguage.Equals(to_language) &&
                !string.IsNullOrEmpty(e.FromLanguage) &&
                e.FromLanguage.Equals(from_language)
            );
            if (translation != null) return true;
            return false;
        }
    }
}

