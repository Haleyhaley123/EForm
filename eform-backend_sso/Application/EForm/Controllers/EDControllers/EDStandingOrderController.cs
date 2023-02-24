﻿using DataAccess.Models;
using DataAccess.Models.EDModel;
using EForm.Authentication;
using EForm.Common;
using EForm.Controllers.BaseControllers;
using EForm.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace EForm.Controllers.EDControllers
{
    [SessionAuthorize]
    public class EDStandingOrderController : BaseEDApiController
    {
        [HttpGet]
        [Route("api/ED/StandingOrder/{id}")]
        [Permission(Code = "ESTOR1")]
        public IHttpActionResult GetEDStandingOrderAPI(Guid id)
        {
            var ed = GetED(id);
            if (ed == null)
                return Content(HttpStatusCode.NotFound, Message.ED_NOT_FOUND);

            var results = unitOfWork.OrderRepository.Find(
                i => !i.IsDeleted &&
                i.VisitId != null &&
                i.VisitId == ed.Id &&
                !string.IsNullOrEmpty(i.OrderType) &&
                i.OrderType.Equals(Constant.ED_STANDING_ORDER)
            )
            .OrderBy(o => o.CreatedAt)
            .Select(o => new
            {
                o.Id,
                o.StandingOrderMasterDataId,
                StandingOrderName = o.StandingOrderMasterData?.Name,
                o.Drug,
                o.Dosage,
                o.Route,
                UsedAt = o.UsedAt?.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND),
                o.MedicalStaffName,
                o.DoctorConfirm,
                o.IsConfirm,
                o.Status,
                o.Note,
                Position = GetListPosotionUserByUserName(o.CreatedBy),
                UpdatedAt = o.UpdatedAt,
                o.UpdatedBy
            }).ToList();

            var lastTime = results.Max(e => e.UpdatedAt);
            var lastObj = results.FirstOrDefault(e => e.UpdatedAt == lastTime);
            return Content(HttpStatusCode.OK, new { Datas = results, LastUpdated = new { lastObj?.UpdatedAt, lastObj?.UpdatedBy } });
        }

        [HttpPost]
        [CSRFCheck]
        [Route("api/ED/StandingOrder/{id}")]
        [Permission(Code = "ESTOR2")]
        public IHttpActionResult UpdateEDStandingOrderAPI(Guid id, [FromBody] JObject request)
        {
            var ed = GetED(id);
            if (ed == null)
                return Content(HttpStatusCode.NotFound, Message.ED_NOT_FOUND);

            var user = GetUser();
            //if (IsBlockAfter24h(ed.CreatedAt) && !HasUnlockPermission(ed.Id, "EDSTOR", user.Username))
            //    return Content(HttpStatusCode.BadRequest, Message.TIME_FORBIDDEN);

            var list_stand = new List<Guid>();
            foreach (var item in request["Datas"])
            {
                string item_id = item["Id"]?.ToString();
                var order = GetOrCreateOrder(item_id, ed.Id);
                if (order != null /*&& IsUserCreateFormManual(user.Username, order.CreatedBy)*/)
                {
                    UpdateOrder(order, item);
                    list_stand.Add(order.Id);
                }
            }
            unitOfWork.Commit();

            if (NeedConfirmOrder(ed.Id))
                CreateConfirmNotification(user.Username, ed);
            return Content(HttpStatusCode.OK, list_stand);
        }

        [HttpPost]
        [CSRFCheck]
        [Route("api/ED/StandingOrder/Confirm/{id}")]
        [Permission(Code = "ESTOR3")]
        public IHttpActionResult ConfirmOrderByDoctorAPI(Guid id, [FromBody] JObject request)
        {
            var order = unitOfWork.OrderRepository.GetById(id);
            if (order == null || order.IsDeleted)
                return Content(HttpStatusCode.NotFound, Message.ORDER_NOT_FOUND);
            if (order.IsConfirm)
                return Content(HttpStatusCode.NotFound, Message.DOCTOR_ACCEPTED);

            var ed = GetED((Guid)order.VisitId);
            if (ed == null)
                return Content(HttpStatusCode.NotFound, Message.ED_NOT_FOUND);

            var username = request["username"]?.ToString();
            var password = request["password"]?.ToString();
            var user = GetAcceptUser(username, password);
            if (user == null)
                return Content(HttpStatusCode.BadRequest, Message.INFO_INCORRECT);

            var action = GetActionOfUser(user, "ESTOR3");
            if (action == null)
                return Content(HttpStatusCode.Forbidden, Message.FORBIDDEN);

            //if (IsBlockAfter24h(ed.CreatedAt) && !HasUnlockPermission(ed.Id, "EDSTOR", user.Username))
            //    return Content(HttpStatusCode.BadRequest, Message.TIME_FORBIDDEN);

            order.DoctorId = user.Id;
            order.DoctorConfirm = user.Username;
            order.IsConfirm = true;
            unitOfWork.OrderRepository.Update(order);
            unitOfWork.Commit();
            return Content(HttpStatusCode.OK, Message.SUCCESS);
        }

        [HttpPost]
        [CSRFCheck]
        [Route("api/ED/StandingOrder/ConfirmAll/{id}")]
        [Permission(Code = "ESTOR4")]
        public IHttpActionResult ConfirmAllOrderByDoctorAPI(Guid id, [FromBody] JObject request)
        {
            var ed = GetED(id);
            if (ed == null)
                return Content(HttpStatusCode.NotFound, Message.ED_NOT_FOUND);

            var username = request["username"]?.ToString();
            var password = request["password"]?.ToString();
            var user = GetAcceptUser(username, password);
            if (user == null)
                return Content(HttpStatusCode.BadRequest, Message.INFO_INCORRECT);

            var action = GetActionOfUser(user, "ESTOR4");
            if (action == null)
                return Content(HttpStatusCode.Forbidden, Message.FORBIDDEN);

            //if (IsBlockAfter24h(ed.CreatedAt) && !HasUnlockPermission(ed.Id, "EDSTOR", user.Username))
            //    return Content(HttpStatusCode.BadRequest, Message.TIME_FORBIDDEN);

            var not_confirm_orders = unitOfWork.OrderRepository.Find(
                i => !i.IsDeleted &&
                i.VisitId != null &&
                i.VisitId == ed.Id &&
                !string.IsNullOrEmpty(i.OrderType) &&
                i.OrderType.Equals(Constant.ED_STANDING_ORDER) &&
                !i.IsConfirm
            ).ToList();

            foreach (var order in not_confirm_orders)
            {
                order.DoctorId = user.Id;
                order.DoctorConfirm = user.Username;
                order.IsConfirm = true;
                unitOfWork.OrderRepository.Update(order);
            }
            unitOfWork.Commit();



            return Content(HttpStatusCode.OK, Message.SUCCESS);
        }

        [HttpPost]
        [CSRFCheck]
        [Route("api/ED/StandingOrder/Delete/{id}")]
        [Permission(Code = "ESTOR5")]
        public IHttpActionResult DeleteOrderAPI(Guid id)
        {
            Order order = unitOfWork.OrderRepository.GetById(id);
            if (order.IsConfirm)
                return Content(HttpStatusCode.BadRequest, Message.SUCCESS);

            var ed = GetED((Guid)order.VisitId);
            if (ed == null)
                return Content(HttpStatusCode.NotFound, Message.ED_NOT_FOUND);

            //var user = GetUser();
            //if (IsBlockAfter24h(ed.CreatedAt) && !HasUnlockPermission(ed.Id, "EDSTOR", user.Username))
            //    return Content(HttpStatusCode.BadRequest, Message.TIME_FORBIDDEN);

            unitOfWork.OrderRepository.Delete(order);
            unitOfWork.Commit();
            return Content(HttpStatusCode.OK, Message.SUCCESS);
        }

        private Order GetOrCreateOrder(string order_id, Guid ed_id)
        {
            if (string.IsNullOrEmpty(order_id))
            {
                Order order = new Order
                {
                    VisitId = ed_id,
                    OrderType = Constant.ED_STANDING_ORDER,
                    MedicalStaffName = GetUser().Username,
                    Status = true
                };
                unitOfWork.OrderRepository.Add(order);
                return order;
            }
            return unitOfWork.OrderRepository.GetById(new Guid(order_id));
        }

        private void UpdateOrder(Order order, JToken item)
        {
            if (!order.IsConfirm)
            {
                var old = new
                {
                    StandingOrderMasterDataId = order.StandingOrderMasterDataId.ToString(),
                    order.Drug,
                    order.Dosage,
                    order.Route,
                    UsedAt = order.UsedAt?.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND),
                    order.Note,
                };
                var _new = new
                {
                    StandingOrderMasterDataId = item["StandingOrderMasterDataId"]?.ToString(),
                    Drug = item["Drug"]?.ToString(),
                    Dosage = item["Dosage"]?.ToString(),
                    Route = item["Route"]?.ToString(),
                    UsedAt = item["UsedAt"]?.ToString(),
                    Note = item["Note"]?.ToString(),
                };

                if (JsonConvert.SerializeObject(old) != JsonConvert.SerializeObject(_new))
                {
                    var st_id = new Guid(_new.StandingOrderMasterDataId);
                    order.StandingOrderMasterDataId = st_id;
                    order.Drug = _new.Drug;
                    order.Dosage = _new.Dosage;
                    order.Route = _new.Route;
                    order.Note = _new.Note;
                    if (!string.IsNullOrEmpty(_new.UsedAt))
                        order.UsedAt = DateTime.ParseExact(_new.UsedAt, Constant.TIME_DATE_FORMAT_WITHOUT_SECOND, null);
                    else
                        order.UsedAt = null;
                    unitOfWork.OrderRepository.Update(order);
                }
            }
        }

        private bool NeedConfirmOrder(Guid visit_id)
        {
            var order = unitOfWork.OrderRepository.FirstOrDefault(
                e => !e.IsDeleted &&
                e.VisitId != null &&
                e.VisitId == visit_id &&
                e.OrderType.Equals(Constant.ED_STANDING_ORDER) &&
                !e.IsConfirm
            );
            if (order != null)
                return true;
            return false;
        }
        private void CreateConfirmNotification(string from_user, ED ed)
        {
            var spec = ed.Specialty;
            var customer = ed.Customer;
            string primary_doctor_user_name = null;
            var discharge = ed.DischargeInformation;
            if (!IsNew(discharge.CreatedAt, discharge.UpdatedAt))
                primary_doctor_user_name = discharge.UpdatedBy;
            if (!string.IsNullOrEmpty(primary_doctor_user_name))
            {
                var vi_mes = string.Format(
                    "<b>[ED - {0}]</b> Bạn có yêu cầu xác nhận <b>Standing Order</b> cho bệnh nhân <b>{1}</b>",
                    spec?.ViName,
                    customer?.Fullname
                );
                var en_mes = string.Format(
                    "<b>[OPD - {0}]</b> Bạn có yêu cầu xác nhận <b>Standing Order</b> cho bệnh nhân <b>{1}</b>",
                    spec?.ViName,
                    customer?.Fullname
                );
                var noti_creator = new NotificationCreator(
                    unitOfWork: unitOfWork,
                    from_user: from_user,
                    to_user: primary_doctor_user_name,
                    priority: 4,
                    vi_message: vi_mes,
                    en_message: en_mes,
                    spec_id: spec?.Id,
                    visit_id: ed.Id,
                    group_code: "ED",
                    form_frontend: "StandingOrder"
                );
                noti_creator.Create();
            }
        }
    }
}
