
using AutoMapper;
using Clients.HisClient;
using DataAccess.Dapper.Repository;
using DataAccess.Models;
using DataAccess.Models.GeneralModel;
using EForm.Authentication;
using EForm.BaseControllers;
using EForm.Common;
using EForm.Services;
using EMRModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace EForm.Controllers
{
    [SessionAuthorize]
    public class DoctorController : ApiController

    {
        // GET: Doctor
        [System.Web.Http.HttpPost]
        [CSRFCheck]
        [System.Web.Http.Route("api/DoctorPlaceDiagnosticsOrder/ChargeV2/{ChargeId}")]
        [Permission(Code = "DRS0007")]
        public async Task<IHttpActionResult> SubmitChargeV3DrsAsync(Guid ChargeId)
        {
            var charge = ExecQuery.getChargeById(ChargeId);
            if (charge == null) return Content(HttpStatusCode.BadRequest, Message.DATA_NOT_FOUND);
            var is_open = await IsVisitOpenV2Async(charge.PatientVisitId.Value);
            
            if (is_open)
            {
                var Oh_Service = new OHServiceDoctor(ChargeId);
                Oh_Service.ChargeItemId = new List<Guid>().ToArray();

                var rsLab = new OHServiceResult();
                var rsRad = new OHServiceResult();
                var rsAll = new OHServiceResult();
                // var listAllChargeItems = unitOfWork.ChargeItemRepository.Find(c => c.ChargeId == ChargeId).ToList();

                rsRad = await Oh_Service.PlaceRadOrderAsync();
                rsAll = await Oh_Service.PlaceAlliedOrderAsync();
                rsLab = await Oh_Service.PlaceLabOrderAsync();

                charge.Status = 1;
                ExecQuery.updateCharge(Mapper.Map<Charge, ChargeDto>(charge));
                //unitOfWork.Commit();
                return Ok(new
                {
                    Total = rsRad.Total + rsAll.Total + rsLab.Total,
                    OK = rsAll.OK + rsLab.OK + rsRad.OK,
                    Failed = rsAll.Failed + rsLab.Failed + rsRad.Failed
                });
            }
            else
            {
                return Content(HttpStatusCode.BadRequest, Message.PLACING_ORDER_VISIT_IS_CLOSED);
            }

        }

        private async Task<bool> IsVisitOpenV2Async(Guid patientVisitId)
        {
            var isOpen = false;
            var visit_from_oh = await HisClient.GetVisitDetailsV2Async(patientVisitId);
            var visit = visit_from_oh.FirstOrDefault();
            if (visit_from_oh != null && visit_from_oh.Count > 0 && visit.ClosureDate == null)
            {
                isOpen = true;
            }
            return isOpen;
        }
    }
}