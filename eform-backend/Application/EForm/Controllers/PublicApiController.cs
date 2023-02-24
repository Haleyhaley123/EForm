using DataAccess;
using DataAccess.Models;
using DataAccess.Models.EIOModel;
using DataAccess.Models.GeneralModel;
using EForm.Authentication;
using EForm.BaseControllers;
using EForm.Client;
using EForm.Common;
using EForm.Models;
using EForm.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Http;
namespace EForm.Controllers
{
    
    public class PublicApiController : BaseApiController
    {
        [SessionAuthorize]
        [HttpPost]
        [Route("api/PublicApi/Charge/UpdateStatus")]
        public IHttpActionResult GetCustomer([FromBody] ChargeUpdateModel request)
        {
            if (request.ChargeId == null || request.ChargeId == Guid.Empty)
                return Content(HttpStatusCode.BadRequest, "Missing parameter ChargeId");
            if (string.IsNullOrEmpty(request.Status))
                return Content(HttpStatusCode.BadRequest, "Missing parameter Status");

            try
            {
                var findChargeItem = unitOfWork.ChargeItemRepository.AsQueryable().Where(c => c.ChargeId == request.ChargeId).ToList();
                if (findChargeItem.Count > 0)
                {
                    foreach (var c in findChargeItem)
                    {
                        if (request.Status == Constant.AlliesRadResponseStatus.Placed)
                        {
                            if (c.Status == Constant.ChargeItemStatus.Placing)
                            {
                                c.Status = Constant.ChargeItemStatus.Placed;
                            }
                            else if (c.Status == Constant.ChargeItemStatus.Cancelling)
                            {
                                c.Status = Constant.ChargeItemStatus.Cancelled;
                            }
                        }
                        else if (request.Status == Constant.AlliesRadResponseStatus.PlaceFailed)
                        {
                            c.Status = Constant.ChargeItemStatus.Failed;
                            c.FailedReason = request.FailedReason;
                        }
                        else if (request.Status == Constant.AlliesRadResponseStatus.CancelFailed)
                        {
                            c.FailedReason = request.FailedReason;
                        }
                        unitOfWork.ChargeItemRepository.Update(c);
                    }

                    unitOfWork.Commit();
                    return Content(HttpStatusCode.OK, new
                    {
                        Message = "Charge updated successfully"
                    });
                }
                else
                {
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        Message = "No charge found"
                    });
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new
                {
                    ex.Message
                });
            }

        }
        [SessionAuthorize]
        [HttpPost]
        [Route("api/PublicApi/Ping")]
        public IHttpActionResult Ping()
        {
            return Content(HttpStatusCode.OK, new
            {
                Message = "Pong"
            });
        }
        [HttpPost]
        [Route("api/eSignCallback")]
        public IHttpActionResult eSignCallback([FromBody] JObject request)
        {
            var stribf = JsonConvert.SerializeObject(request);
            LogTmp logTmp = new LogTmp
            {
                Id = Guid.NewGuid(),
                UpdatedAt = DateTime.Now,
                CreatedAt = DateTime.Now,
                Ip = ConfigurationManager.AppSettings["APP_ID"] != null ? ConfigurationManager.AppSettings["APP_ID"].ToString() : "NONE",
                URI = "api/eSignCallback",
                Action = "eSignCallback",
                Request = stribf,
                Response = "",
            };
            unitOfWork.LogTmpRepository.Add(logTmp);
            unitOfWork.Commit();
            var signby = request["SignBy"]?.ToString();
            var signat = request["SignAt"]?.ToString();
            var b_Id = request["b_Id"]?.ToString();
            var b_dvi = request["b_dvi"]?.ToString();
            var b_vaitro = request["b_vaitro"]?.ToString();
            var type = request["type"]?.ToString();
            var locationKey = request["locationKey"]?.ToString();
            var base64_data = request["dataBase64"]?.ToString();
            bool isGuidId = Guid.TryParse(b_Id,out Guid id);
            bool issignat = DateTime.TryParse(signat, null, DateTimeStyles.None, out DateTime _signat);
                var esign = unitOfWork.EsignRepository.FirstOrDefault(e =>!e.IsDeleted && e.Id == id);
            if(esign != null && issignat && isGuidId)
            {
                EIOFormConfirm new_confirm = new EIOFormConfirm()
                {
                    FormId = esign.FormId,
                    ConfirmType = locationKey,
                    ConfirmAt = _signat,
                    ConfirmBy = signby,
                    Note = "ký tử esign",
                    EsignId = id

                };
                unitOfWork.EIOFormConfirmRepository.Add(new_confirm);
                unitOfWork.Commit();
                SaveFileEsign(esign.VisitId, id, base64_data, esign.FormCode);
            }

            return Content(HttpStatusCode.OK, new
            {
                Message = "Đã nhận thông tin"
            });
        }
        [HttpGet]
        [Route("api/eSignCallback")]
        public IHttpActionResult eSignCallbackList()
        {
            var logs = unitOfWork.LogTmpRepository.Find(e => e.Action == "eSignCallback").OrderByDescending(e => e.CreatedAt).Take(10).ToList();
            return Content(HttpStatusCode.OK, new
            {
                Data = logs
            });
        }
    }
}