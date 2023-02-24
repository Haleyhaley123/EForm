using EForm.Authentication;
using EForm.Controllers.EIOControllers;
using EForm.Controllers.GeneralControllers;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.ServiceModel.Channels;
using System.Web.Http;
using EForm.Common;
using DataAccess.Models.EIOModel;
using EForm.BaseControllers;
using System.Linq;
using System.Web;

namespace EForm.Controllers.EIOControllers
{
    [SessionAuthorize]
    public class EMRFormController : BaseApiController
    {
        [HttpGet]
        [Route("api/GetList/{type}/{formcode}/{visitId}")]
        [Permission(Code = "APIGDT")]
        public IHttpActionResult GetList(string type, string formcode, Guid visitId)
        {
            var formcodeRepo = ConvertFormCode(formcode);
            var checkfrom = unitOfWork.FormRepository.FirstOrDefault(e => e.VisitTypeGroupCode == type && e.Code == formcodeRepo);
            if (checkfrom == null) return Content(HttpStatusCode.NotFound, new { ViName = "Không tìm thấy formcode! ", EnName = "Formcode is not found!" });
            var visit = GetVisit(visitId, type);
            if (visit == null)
                return Content(HttpStatusCode.NotFound, Common.Message.VISIT_NOT_FOUND);
            bool islock = false;

            if (type == "IPD")
            {
                islock = IPDIsBlock(visit, formcodeRepo);
            }
            if (type == "OPD")
            {
                var user = GetUser();
                islock = Is24hLocked(visit.CreatedAt, visit.Id, formcodeRepo, user.Username);
            }
            var forms = GetForms(visitId, formcode, type);
            if (forms.Count == 0)
                return Content(HttpStatusCode.NotFound, new
                {
                    visitId = visitId,
                    IsLock24h = islock,
                    Common.Message.FORM_NOT_FOUND
                });
            var datas = forms.Select(form => new
            {
                form?.Id,
                form?.CreatedAt,
                form?.CreatedBy,
                form?.UpdatedAt,
                form?.UpdatedBy
            }).OrderBy(e => e.CreatedAt).ToList();
            var lastUpdate = datas.OrderByDescending(x => x.UpdatedAt).FirstOrDefault();
            return Content(HttpStatusCode.OK, new
            {
                VisitId = visitId,
                Datas = datas,
                LastInfo = new
                {
                    lastUpdate?.CreatedAt,
                    lastUpdate?.CreatedBy,
                    lastUpdate?.UpdatedAt,
                    lastUpdate?.UpdatedBy
                },
                IsLock24h = islock,
            });
        }
        [HttpGet]
        [Route("api/GetDetail/{type}/{formcode}/{visitId}/{id}")]
        [Permission(Code = "APIGDT")]
        public IHttpActionResult GetFormByVisitId(string type, string formcode, Guid visitId, Guid id)
        {
            var formcodeRepo = ConvertFormCode(formcode);
            var checkfrom = unitOfWork.FormRepository.FirstOrDefault(e => e.VisitTypeGroupCode == type && e.Code == formcodeRepo);
            if (checkfrom == null) return Content(HttpStatusCode.NotFound, new { ViName = "Không tìm thấy formcode! ", EnName = "Formcode is not found!" });
            var visit = GetVisit(visitId, type);
            if (visit == null)
                return Content(HttpStatusCode.NotFound, Common.Message.VISIT_NOT_FOUND);

            var form = unitOfWork.EIOFormRepository.GetById(id);
            if (form == null)
                return Content(HttpStatusCode.NotFound, Common.Message.FORM_NOT_FOUND);


            bool IsLock24h = false;
            var isUnLockConfirm = unitOfWork.UnlockFormToUpdateRepository
                                            .Find(e => e.FormCode == formcodeRepo && e.FormId != null && e.FormId == form.Id && e.ExpiredAt >= DateTime.Now)
                                            .OrderByDescending(e => e.UpdatedAt)?.FirstOrDefault() != null ? true : false;
            if (type == "IPD")
            {
                IsLock24h = IPDIsBlock(visit, formcodeRepo, id);
            }
            if (type == "OPD")
            {
                var user = GetUser();
                if (formcode == "A01_067_050919_VE_TAB1")
                {
                    if (isUnLockConfirm)
                    {
                        IsLock24h = false;
                    }
                    else
                    {
                        IsLock24h = Is24hLocked(visit.CreatedAt, visit.Id, formcodeRepo, user.Username, id);
                    }
                }
                else
                {
                    IsLock24h = Is24hLocked(visit.CreatedAt, visit.Id, formcodeRepo, user.Username, id);
                }

            }
            var data = FormatOutput(form, visit, isUnLockConfirm);
            return Content(HttpStatusCode.OK, new { data, IsLock24h = IsLock24h });
        }
        [HttpPost]
        [Route("api/CreateForm/{type}/{formcode}/{visitId}")]
        [Permission(Code = "APICR")]
        public IHttpActionResult CreateForm(string type, string formcode, Guid visitId)
        {
            var formcodeRepo = ConvertFormCode(formcode);
            var checkfrom = unitOfWork.FormRepository.FirstOrDefault(e => e.VisitTypeGroupCode == type && e.Code == formcodeRepo);
            if (checkfrom == null) return Content(HttpStatusCode.NotFound, new { ViName = "Không tìm thấy formcode! ", EnName = "Formcode is not found!" });
            var visit = GetVisit(visitId, type);
            if (visit == null)
                return Content(HttpStatusCode.NotFound, Common.Message.VISIT_NOT_FOUND);

            if (Validate24hLocked(visit, type, formcodeRepo))
                return Content(HttpStatusCode.BadRequest, Common.Message.FORM_IS_LOCKED);

            if (checkfrom.Time == 1)
            {
                var form = unitOfWork.EIOFormRepository.FirstOrDefault(e => e.FormCode == formcode && e.VisitTypeGroupCode == type && e.VisitId == visitId);
                if (form != null) return Content(HttpStatusCode.BadRequest, Common.Message.FORM_EXIST);
            }
            var form_data = new EIOForm
            {
                VisitId = visitId,
                Version = checkfrom?.Version,
                VisitTypeGroupCode = type,
                FormCode = formcode
            };
            unitOfWork.EIOFormRepository.Add(form_data);
            unitOfWork.Commit();
            UpdateVisit(visit, type);

            return Content(HttpStatusCode.OK, new { form_data.Id, form_data.CreatedAt, form_data.CreatedBy, form_data.VisitId });
        }

        [HttpPost]
        [Route("api/UpdateForm/{type}/{formcode}/{visitId}/{id}")]
        [Permission(Code = "APIUD")]
        public IHttpActionResult UpdateForm(string type, string formcode, Guid visitId, Guid id, [FromBody] JObject request)
        {
            var formcodeRepo = ConvertFormCode(formcode);
            var checkfrom = unitOfWork.FormRepository.FirstOrDefault(e => e.VisitTypeGroupCode == type && e.Code == formcodeRepo);
            if (checkfrom == null) return Content(HttpStatusCode.NotFound, new { ViName = "Không tìm thấy formcode! ", EnName = "Formcode is not found!" });
            var visit = GetVisit(visitId, type);
            if (visit == null)
                return Content(HttpStatusCode.NotFound, Common.Message.VISIT_NOT_FOUND);

            var form = unitOfWork.EIOFormRepository.Find(e => !e.IsDeleted && e.VisitId == visitId && e.FormCode == formcode && e.Id == id).FirstOrDefault(); ;
            if (form == null)
                return Content(HttpStatusCode.NotFound, Common.Message.FORM_NOT_FOUND);

            if (formcode == "A01_067_050919_VE_TAB1")
            {
                var isUnLockConfirm = unitOfWork.UnlockFormToUpdateRepository
                                                .Find(e => e.FormCode == formcodeRepo && e.FormId != null && e.FormId == form.Id && e.ExpiredAt >= DateTime.Now)
                                                .OrderByDescending(e => e.UpdatedAt)?.FirstOrDefault() != null ? true : false;
                if (isUnLockConfirm)
                {
                    if (GetFormConfirms(form.Id).Count > 0)
                    {
                        return Content(HttpStatusCode.Forbidden, Common.Message.OWNER_FORBIDDEN);
                    }
                }
                else
                {
                    if (Validate24hLocked(visit, type, formcodeRepo, form.Id))
                        return Content(HttpStatusCode.BadRequest, Common.Message.FORM_IS_LOCKED);
                }
            }
            else
            {
                if (Validate24hLocked(visit, type, formcodeRepo, form.Id))
                    return Content(HttpStatusCode.BadRequest, Common.Message.FORM_IS_LOCKED);

                if (checkfrom.Ispermission)
                {
                    var user = GetUser();
                    if (user.Username != form.CreatedBy && !IsCheckConfirm(form.Id))
                    {
                        return Content(HttpStatusCode.Forbidden, Common.Message.OWNER_FORBIDDEN);
                    }
                }
            }
            HandleUpdateOrCreateFormDatas((Guid)form.VisitId, form.Id, formcode, request["Datas"]);

            form.Note = request["Note"]?.ToString();
            form.Comment = request["Comment"]?.ToString();
            unitOfWork.EIOFormRepository.Update(form);

            UpdateVisit(visit, type);

            return Content(HttpStatusCode.OK, new { form.Id });
        }

        [HttpPost]
        [Route("api/ConfirmForm/{type}/{formcode}/{visitId}/{idform}")]
        [Permission(Code = "APICF")]
        public IHttpActionResult ConfirmForm(string type, string formcode, Guid visitId, Guid idform, [FromBody] JObject request)
        {
            var formcodeRepo = ConvertFormCode(formcode);
            var checkfrom = unitOfWork.FormRepository.FirstOrDefault(e => e.VisitTypeGroupCode == type && e.Code == formcodeRepo);
            if (checkfrom == null) return Content(HttpStatusCode.NotFound, new { ViName = "Không tìm thấy formcode! ", EnName = "Formcode is not found!" });
            var visit = GetVisit(visitId, type);
            if (visit == null)
                return Content(HttpStatusCode.BadRequest, "Không tồn tại");

            var form = unitOfWork.EIOFormRepository.FirstOrDefault(e => !e.IsDeleted && e.VisitId == visitId && e.Id == idform);
            if (form == null)
                return Content(HttpStatusCode.NotFound, Common.Message.FORM_NOT_FOUND);
            if (formcode == "A01_067_050919_VE_TAB1")
            {
                var isUnLockConfirm = unitOfWork.UnlockFormToUpdateRepository
                                                .Find(e => e.FormCode == formcodeRepo && e.FormId != null && e.FormId == form.Id && e.ExpiredAt >= DateTime.Now)
                                                .OrderByDescending(e => e.UpdatedAt)?.FirstOrDefault() != null ? true : false;
                if (isUnLockConfirm)
                {
                    if (GetFormConfirms(form.Id).Count > 0)
                    {
                        return Content(HttpStatusCode.Forbidden, Common.Message.OWNER_FORBIDDEN);
                    }
                }
                else
                {
                    if (Validate24hLocked(visit, type, formcodeRepo, form.Id))
                        return Content(HttpStatusCode.BadRequest, Common.Message.FORM_IS_LOCKED);
                }
            }
            else
            {
                if (Validate24hLocked(visit, type, formcodeRepo, form.Id))
                    return Content(HttpStatusCode.BadRequest, Common.Message.FORM_IS_LOCKED);
            }


            var username = request["username"]?.ToString();
            var password = request["password"]?.ToString();
            var kind = request["kind"]?.ToString();
            var user = GetAcceptUser(username, password);
            if (user == null)
                return Content(HttpStatusCode.BadRequest, Common.Message.INFO_INCORRECT);
            var PermissionCode = "APICF" + type + formcodeRepo;
            var ischeckpermission = ICheckPermission(username, PermissionCode);
            if (!ischeckpermission)
                return Content(HttpStatusCode.Forbidden, Common.Message.OWNER_FORBIDDEN);
            Guid? formid = form.Id;
            var getconfirm = GetFormConfirms(form.Id);
            if (getconfirm.Count > 0)
            {
                foreach (var item in getconfirm)
                {
                    if (kind != item.ConfirmType)
                    {
                        SaveConfirm(username, kind, formid);
                    }
                    else
                    {
                        return Content(HttpStatusCode.BadRequest, Common.Message.INFO_INCORRECT);
                    }
                }
            }
            else
            {
                SaveConfirm(username, kind, formid);
            }
            UpdateVisit(visit, type);

            return Content(HttpStatusCode.OK, Common.Message.SUCCESS);

        }

        private bool Validate24hLocked(dynamic visit, string type, string formCode, Guid? formId = null)
        {
            if (type == "OPD")
            {
                var user_login = GetUser();
                return Is24hLocked(visit.CreatedAt, visit.Id, formCode, user_login?.Username, formId);
            }
            else if (type == "IPD")
                return IPDIsBlock(visit, formCode);
            else
                return false;
        }
    }
}