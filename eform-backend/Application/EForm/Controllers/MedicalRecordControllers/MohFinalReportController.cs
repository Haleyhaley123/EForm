using Bussiness.HisService;
using DataAccess;

using EForm.Authentication;
using EForm.BaseControllers;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Web.Http;

namespace EForm.Controllers.MedicalRecordControllers
{
    [SessionAuthorize]
    public class MohFinalReportController : BaseApiController {
        [HttpGet]
        [Route("api/ApiReport/MohFinalReport")]
        [Permission(Code = "XEMCLSBYPID")]
        public async System.Threading.Tasks.Task<IHttpActionResult> MohFinalReportAsync(string PID, string visitCode, string siteCode)
        {
            var vihc_report = await OHAPIService.GetMohFinalReportAsync(PID, visitCode, siteCode);
            return Content(HttpStatusCode.OK, JToken.Parse(vihc_report.ToString()));
        }
    }
}