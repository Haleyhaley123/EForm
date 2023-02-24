using Common;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace EForm.Controllers
{
    public class AccountController : Controller
    {
        //// GET: Account
        [Authorize]
        public JsonResult SessionChanged()
        {
            // If the javascript made the reuest, issue a challenge so the OIDC request will be constructed.
            if (HttpContext.GetOwinContext().Request.QueryString.Value == "")
            {
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties { RedirectUri = "/Account/SessionChanged" },
                    OpenIdConnectAuthenticationDefaults.AuthenticationType);
                Request.GetOwinContext().Authentication.SignOut();
                //Request.GetOwinContext().Authentication.SignOut(Microsoft.AspNet.Identity.DefaultAuthenticationTypes.ApplicationCookie);
                this.HttpContext.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
                return Json(new { }, "application/json", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var m = HttpContext.GetOwinContext().Request.QueryString.Value;
                // 'RedirectToIdentityProvider' redirects here with the OIDC request as the query string
                return Json(HttpContext.GetOwinContext().Request.QueryString.Value, "application/json", JsonRequestBehavior.AllowGet);
            }
        }


        // Action for displaying a page notifying the user that they've been signed out automatically.
        public ActionResult SingleSignOut(string redirectUri)
        {
            // RedirectUri is necessary to bring a user back to the same location 
            // if they re-authenticate after a single sign out has occurred. 
            if (redirectUri == null)
                ViewBag.RedirectUri = Startup.PostLogoutRedirectUri;
            else
                ViewBag.RedirectUri = redirectUri;
            HttpContext.GetOwinContext().Authentication.SignOut();
            HttpContext.GetOwinContext().Authentication.User =
                new GenericPrincipal(new GenericIdentity(string.Empty), null);
            HttpContext.GetOwinContext().Authentication.SignOut(
            OpenIdConnectAuthenticationDefaults.AuthenticationType,
            CookieAuthenticationDefaults.AuthenticationType);
            return RedirectToAction("Login","Authen");
            //return null;
        }

        // Sign in has been triggered from Sign In Button or From Single Sign Out Page.
        public void SignIn(string redirectUri)
        {
            // RedirectUri is necessary to bring a user back to the same location 
            // if they re-authenticate after a single sign out has occurred.
            if (redirectUri == null)
                redirectUri = "/";
            if (!Request.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties { RedirectUri = redirectUri },
                    OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
        }

        public void EndSession()
        {
            Request.GetOwinContext().Authentication.SignOut();
            //Request.GetOwinContext().Authentication.SignOut(Microsoft.AspNet.Identity.DefaultAuthenticationTypes.ApplicationCookie);
            this.HttpContext.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
        }

        // Sign a user out of both AAD and the Application
        public void SignOut()
        {
          
            HttpContext.GetOwinContext().Authentication.SignOut(
                new AuthenticationProperties { RedirectUri = Startup.PostLogoutRedirectUri},
                OpenIdConnectAuthenticationDefaults.AuthenticationType,
                CookieAuthenticationDefaults.AuthenticationType);
            

        }
       
        public ActionResult RedirectLogin(string Token)
        {

            ViewBag.token = Token;
            return View();
        }

    }
}