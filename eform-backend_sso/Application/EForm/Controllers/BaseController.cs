using Bussiness.IPD;
using DataAccess.Repository;
using EForm.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace EForm.Controllers
{
    public class BaseController : Controller
    {


        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
           string controllerName = filterContext.HttpContext.Request.RequestContext.RouteData.Values["controller"].ToString();
            string actionName = filterContext.HttpContext.Request.RequestContext.RouteData.Values["action"].ToString();
            var cookieCheckLogin = filterContext.HttpContext.Request.Cookies["_CookieCheckLogin"];
            var users = filterContext.HttpContext.Request.GetOwinContext().Authentication.User.Identity;
            
          
            var value = filterContext.HttpContext.Request.Cookies["EFormED"];
            if (value == null || value.Value == "")
            {
                string url = new UrlHelper(filterContext.HttpContext.Request.RequestContext).Action("Login", "Authen");
                if (filterContext.HttpContext.Request.IsAjaxRequest() && filterContext.HttpContext.Request.HttpMethod == "POST")
                {
                    url = new UrlHelper(filterContext.HttpContext.Request.RequestContext).Action("Login", "Authen");
                    filterContext.Result = new JsonResult
                    {
                        Data = new { redirect = url, status = 401 },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    var ck = System.Web.HttpContext.Current.User.Identity.Name;
                    if (ck == "" && cookieCheckLogin == null)
                    {
                        filterContext.Result = new RedirectResult(url);
                        
                    }
                    else
                    {
                        var result = Startup.email.Split('@');
                        var st = result[0].ToString();
                        var isLogon = ApiHelper.ChecloginEMR(st, Startup.sessionId.ToString());
                        if (isLogon == "0")
                        {

                            filterContext.Result = new RedirectResult(url);
                        }
                        else if (isLogon == "1")
                        {
                            string url1 = new UrlHelper(filterContext.HttpContext.Request.RequestContext).Action("DoLogin", "Authen");
                            filterContext.Result = new RedirectResult(url1);
                        }
                        //string url1 = new UrlHelper(filterContext.HttpContext.Request.RequestContext).Action("DoLogin", "Authen");
                        //filterContext.Result = new RedirectResult(url1);
                    }
    
                    //filterContext.Result = new RedirectResult(url);
                }

            }
            else
            {
                var cookie = filterContext.HttpContext.Request.Cookies[".AspNet.Cookies"];
                if (cookie == null)
                {
                    var url1 = new UrlHelper(filterContext.HttpContext.Request.RequestContext).Action("SignOut", "Authen");
                    filterContext.Result = new RedirectResult(url1);
                }
                else
                {
                  
                }
            }
            //else
            //{
            //    filterContext.Result = new RedirectResult(Url.Action("DoLogin", "Authen"));
            //}
            //string curRequestUrl = HttpUtility.UrlEncode(Request.Url.ToString());
            //var m = filterContext.HttpContext.Request.HttpMethod == "POST";

            //if (controllerName == "Home" && actionName == "Index")
            //{
            //    var cookieCheckLogin = filterContext.HttpContext.Request.Cookies["_CookieCheckLogin"];
            //    var cookie1 = HttpContext.Request.Cookies[".AspNet.Cookies"];
            //    if (cookieCheckLogin != null)
            //    {
            //        filterContext.Result = null;
            //    }
            //    else
            //    {

            //        var cookie = filterContext.HttpContext.Request.Cookies[".AspNet.Cookies"];
            //        if (cookie == null)
            //        {
            //            filterContext.HttpContext.Request.GetOwinContext().Authentication.User =
            //           new GenericPrincipal(new GenericIdentity(string.Empty), null);
            //            if (!filterContext.HttpContext.Request.IsAuthenticated)
            //            {
            //                filterContext.Result = null;
            //            }
            //        }

            //        else
            //        {
            //            if (!filterContext.HttpContext.Request.IsAuthenticated)
            //            {
            //                filterContext.Result = null;
            //            }
            //            else
            //            {
            //                filterContext.Result = new RedirectResult(Url.Action("DoLogin", "Home"));
            //            }

            //        }
            //    }

            //}

        }
    }
}