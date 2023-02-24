using Admin.App_Start;
using Admin.Common.Model;
using Admin.CustomAuthen;
using Common;
using DataAccess.Models;
using DataAccess.Repository;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using Newtonsoft.Json;
using System;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Admin.Controllers
{

   [RedirectingAction]
    public class HomeController : Controller
    {

        protected IUnitOfWork unitOfWork = new EfUnitOfWork();
        [Authorize]
        public ActionResult Index()
        {
            
            var  value = HttpContext.Request.Cookies[FormsAuthentication.FormsCookieName].Value;
            return View();
        }
        public ActionResult Logins()
        {
            HttpContext.GetOwinContext().Authentication.Challenge(
                   new AuthenticationProperties { RedirectUri = "/" },
                   OpenIdConnectAuthenticationDefaults.AuthenticationType);
            return null;
        }


    }

}