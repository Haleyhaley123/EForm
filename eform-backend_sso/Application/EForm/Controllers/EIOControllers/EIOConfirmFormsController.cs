using DataAccess.Model.BaseModel;
using DataAccess.Models.EIOModel;
using EForm.Authentication;
using EForm.BaseControllers;
using EForm.Common;
using EForm.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace EForm.Controllers.EIOControllers
{
    [SessionAuthorize]
    public class EIOConfirmFormsController : BaseApiController
    {
        [HttpGet]
        [Route("api/EIO/ConfirmForms/GetListIDConfirm/{visitId}/{formCode}")]
        IHttpActionResult GetIdConfirms(Guid visitId, string formCode)
        {
            var result =(from form in unitOfWork.EIOFormRepository.AsQueryable()
                         .Where(e => !e.IsDeleted && e.VisitId == visitId && e.FormCode == formCode)
                         join confirm in unitOfWork.EIOFormConfirmRepository.AsQueryable()
                         .Where(e => !e.IsDeleted)
                         on form.Id equals confirm.FormId
                         select new
                         {
                             confirm.FormId,
                             confirm.Note,
                             confirm.ConfirmAt,
                             confirm.ConfirmBy,
                             confirm.ConfirmType
                         }).ToArray();

            return Content(HttpStatusCode.OK, new { Confirm = result });
        }

        [HttpGet]
        [Route("api/EIO/ConfirmForms/GetList/{formId}/{*note}")]
        public IHttpActionResult GetConfirmFormByFormId(Guid formId, string note)
        {
            var confirm = (from c in unitOfWork.EIOFormConfirmRepository.AsQueryable()
                           where !c.IsDeleted && c.FormId == formId && c.Note == note
                           join use in unitOfWork.UserRepository.AsQueryable()
                           .Where(m => !m.IsDeleted)
                           on c.ConfirmBy equals use.Username
                           select new
                           {
                               c.Id,
                               c.ConfirmAt,
                               c.ConfirmBy,
                               use.Fullname,
                               use.Title,
                               c.ConfirmType,
                               c.FormId,
                               c.Note
                           }).ToList();

            return Content(HttpStatusCode.OK, confirm);
        }
        [HttpGet]
        [Route("api/EIO/ConfirmForms/GetDetail/{formId}/{*note}")]
        public IHttpActionResult GetConfirmDetailByFormId(Guid formId, string note)
        {
            return Content(HttpStatusCode.OK, GetEIOFormConfirmByFormId(formId, note));
        }

        private object GetEIOFormConfirmByFormId(Guid formId, string note)
        {
            //throw new NotImplementedException();
            return null;
        }

        [HttpPost]
        [Route("api/EIO/ConfirmForms/Created/{visitId}/{formId}/{formCode}")]
        public IHttpActionResult CreatedConfirmByFormId(Guid visitId ,Guid formId, string formCode, [FromBody] JObject request)
        {
            string visitTypeCode = GetCurrentVisitType();
            var visit = GetVisit(visitId, visitTypeCode);
            if (visit == null)
                return Content(HttpStatusCode.NotFound, Message.VISIT_NOT_FOUND);

            string req_userName = request["Username"]?.ToString();
            string req_passWord = request["Password"]?.ToString();
            string req_kind = request["Kind"]?.ToString();
            string note = request["Note"]?.ToString();

            var user = GetAcceptUser(req_userName, req_passWord);
            if (user == null)
                return Content(HttpStatusCode.BadRequest, Message.INFO_INCORRECT);

            if(req_kind.ToUpper().Contains("USERCREATED"))
                if (!Confirm(visitId, formId, formCode, user.Username, note))
                    return Content(HttpStatusCode.Forbidden, Message.FORBIDDEN);

            bool success = CreateConfirmUserCreatedForm(visit, formId, visitTypeCode, formCode, req_kind, user.Username, note);
            if (!success)
                return Content(HttpStatusCode.BadRequest, Message.INFO_INCORRECT);

            return Content(HttpStatusCode.OK, Message.SUCCESS);
        }

        private bool CreateConfirmUserCreatedForm(dynamic visit, Guid formId, string visitTypeCode, string formCode, string kind, string userName, string note)
        {

            var eio_form = unitOfWork.EIOFormRepository.GetById(formId);
            if (eio_form == null)
            {
                eio_form = new EIOForm()
                {
                    Id = formId,
                    VisitTypeGroupCode = visitTypeCode,
                    VisitId = visit?.Id,
                    FormCode = formCode,
                    Version = visit?.Version
                };
                unitOfWork.EIOFormRepository.Add(eio_form);
            }
            var confirm = unitOfWork.EIOFormConfirmRepository
                          .FirstOrDefault(e => !e.IsDeleted && e.FormId == eio_form.Id
                                                && e.ConfirmType != null && e.ConfirmType.ToUpper() == kind.ToUpper()
                                                && e.Note == note
                                             );

            if (confirm != null)
                return false;

            EIOFormConfirm new_confirm = new EIOFormConfirm()
            {
                FormId = eio_form.Id,
                ConfirmType = kind.ToUpper(),
                ConfirmAt = DateTime.Now,
                ConfirmBy = userName,
                Note = note
            };
            unitOfWork.EIOFormConfirmRepository.Add(new_confirm);
            unitOfWork.Commit();
            return true;
        }

        private Guid StringToGuid(string input)
        {
            Guid gui;
            bool success = Guid.TryParse(input, out gui);
            return gui;
        }

        private bool ConfirmSuccess(dynamic form, string userName)
        {
            if (form == null)
                return false;

            if (form.CreatedBy?.ToLower() == userName.ToLower())
                return true;
            return false;
        }

        private bool Confirm(Guid visitId, Guid formId, string formCode, string userName, string note)
        {
            Guid id_object = StringToGuid(note);
            dynamic form = null;
            switch (formCode)
            {
                case "A03_027_080322_V":
                    form = unitOfWork.EIOBloodProductDataRepository.FirstOrDefault(e => !e.IsDeleted && e.Id == formId);
                    return ConfirmSuccess(form, userName);
                case "A03_038_080322_V":
                    form = unitOfWork.EDArterialBloodGasTestRepository.FirstOrDefault(e => !e.IsDeleted && e.Id == formId);
                    return ConfirmSuccess(form, userName);
                case "A03_039_080322_V":
                    form = unitOfWork.EDChemicalBiologyTestRepository.FirstOrDefault(e => !e.IsDeleted && e.Id == formId);
                    return ConfirmSuccess(form, userName);
                case "OPDIAFOGOP":
                    form = unitOfWork.OPDInitialAssessmentForOnGoingRepository.FirstOrDefault(e => !e.IsDeleted && e.Id == formId);
                    return ConfirmSuccess(form, userName);
                case "A03_029_050919_VE":
                    form = unitOfWork.OrderRepository.FirstOrDefault(e => !e.IsDeleted && e.Id == formId);
                    return ConfirmSuccess(form, userName);
                case "A02_066_050919_VE":
                    form = unitOfWork.IPDGlamorganRepository.FirstOrDefault(e => !e.IsDeleted && e.Id == formId);
                    return ConfirmSuccess(form, userName);
                case "A02_069_080121_VE":
                    form = unitOfWork.IPDInitialAssessmentForAdultRepository.FirstOrDefault(e => !e.IsDeleted && e.Id == formId);
                    return ConfirmSuccess(form, userName);
                case "A02_014_220321_VE":
                    form = unitOfWork.IPDInitialAssessmentForAdultRepository.FirstOrDefault(e => !e.IsDeleted && e.Id == formId);
                    return ConfirmSuccess(form, userName);
                case "A02_015_220321_VE":
                    form = unitOfWork.IPDInitialAssessmentForAdultRepository.FirstOrDefault(e => !e.IsDeleted && e.Id == formId);
                    return ConfirmSuccess(form, userName);
                case "A02_008_080121_VE":
                    form = unitOfWork.EOCInitialAssessmentForOnGoingRepository.FirstOrDefault(e => !e.IsDeleted && e.Id == formId);
                    return ConfirmSuccess(form, userName);
                case "OPDIAFTP":
                    form = unitOfWork.OPDInitialAssessmentForTelehealthRepository.FirstOrDefault(e => !e.IsDeleted && e.Id == formId);
                    return ConfirmSuccess(form, userName);
                default:
                    form = unitOfWork.EIOFormRepository.FirstOrDefault(e => !e.IsDeleted && e.Id == formId && e.FormCode == formCode);
                    return ConfirmSuccess(form, userName);
            }
        }
    }
}
