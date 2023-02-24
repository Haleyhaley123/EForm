using Admin.Common.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Admin.Controllers
{
    public class BaseController : Controller
    {
        // GET: Base
        //private string urlReferrer = null;
        //protected string UrlReferrer
        //{
        //    get { return urlReferrer; }
        //}

        //public override void OnActionExecuting(ActionExecutingContext filterContext)
        //{
        //    base.OnActionExecuting(filterContext);

        //    string controllerName = filterContext.HttpContext.Request.RequestContext.RouteData.Values["controller"].ToString();
        //    string actionName = filterContext.HttpContext.Request.RequestContext.RouteData.Values["action"].ToString();
        //    var value = "";
        //    if (filterContext.HttpContext.Request.Cookies[FormsAuthentication.FormsCookieName] != null)
        //    {
        //        value = filterContext.HttpContext.Request.Cookies[FormsAuthentication.FormsCookieName].Value;
        //    }
        //    UserData memberBaseInfo = CurrentUser(value);
        //    if (memberBaseInfo == null)
        //    {
        //        SignOutLocal(filterContext.HttpContext);
        //        string url = new UrlHelper(filterContext.HttpContext.Request.RequestContext).Action("Login", "Authen");

        //        if (filterContext.HttpContext.Request.IsAjaxRequest() && filterContext.HttpContext.Request.HttpMethod == "POST")
        //        {
        //            url = new UrlHelper(filterContext.HttpContext.Request.RequestContext).Action("Login", "Authen");
        //            filterContext.Result = new JsonResult
        //            {
        //                Data = new { redirect = url, status = 401 },
        //                JsonRequestBehavior = JsonRequestBehavior.AllowGet
        //            };
        //        }
        //        else if (controllerName == "Home" && actionName == "Index" && filterContext.HttpContext.Request.HttpMethod == "POST")
        //        {
        //            filterContext.Result = null;
        //        }
        //        else
        //        {
        //            filterContext.Result = new RedirectResult(url);
        //        }
        //    }


        //}

        //public static UserData CurrentUser(string value)
        //{
        //    UserData userApp = null;
        //    var LoginTokenCookie = value;
        //    if (LoginTokenCookie != null)
        //    {
        //        try
        //        {
        //            var token = FormsAuthentication.Decrypt(LoginTokenCookie);
        //            userApp = JsonConvert.DeserializeObject<UserData>(token.UserData);
        //            if (userApp == null)
        //            {

        //                return null;
        //            }
        //        }
        //        catch (Exception ex)
        //        {

        //        }

        //    }
        //    return userApp;
        //}
        //private void SignOutLocal(HttpContextBase httpContext)
        //{


        //    FormsAuthentication.SignOut();
        //    httpContext.Session.Abandon();
        //    HttpContext.Current.Session.Abandon();
        //    //RemoveUserModel(httpContext);
        //}

    }
}