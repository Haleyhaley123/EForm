using Admin.Common.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Admin.App_Start
{
    public class RedirectingAction : ActionFilterAttribute
    {
        private string urlReferrer = null;
        protected string UrlReferrer
        {
            get { return urlReferrer; }
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
      {
            base.OnActionExecuting(filterContext);
            var m = filterContext.HttpContext.Request.Url;
            Uri myUri = new Uri(m.ToString(), UriKind.RelativeOrAbsolute);
            string param1 = HttpUtility.ParseQueryString(myUri.Query).Get("ReturnUrl");
            string controllerName = filterContext.HttpContext.Request.RequestContext.RouteData.Values["controller"].ToString();
            string actionName = filterContext.HttpContext.Request.RequestContext.RouteData.Values["action"].ToString();
            var value = "";
            
           
            if (filterContext.HttpContext.Request.Cookies[FormsAuthentication.FormsCookieName] != null)
            {
                value = filterContext.HttpContext.Request.Cookies[FormsAuthentication.FormsCookieName].Value;
            }
            UserData memberBaseInfo = CurrentUser(value);
            
            if (memberBaseInfo == null)
            {
                
                SignOutLocal(filterContext.HttpContext);
                string url = new UrlHelper(filterContext.HttpContext.Request.RequestContext).Action("DoLogin", "Account");
                filterContext.Result = new RedirectResult(url);
                //if (filterContext.HttpContext.Request.IsAjaxRequest() && filterContext.HttpContext.Request.HttpMethod == "POST")
                //{
                //    url = new UrlHelper(filterContext.HttpContext.Request.RequestContext).Action("DoLogin", "Home");
                //    filterContext.Result = new JsonResult
                //    {
                //        Data = new { redirect = url, status = 401 },
                //        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                //    };
                //}
                //else if (controllerName == "Home" && actionName == "Index" && filterContext.HttpContext.Request.HttpMethod == "POST")
                //{
                //    filterContext.Result = null;
                //}
                //else
                //{
                //    filterContext.Result = new RedirectResult(url);
                //}
            }
            else
            {
                var cookie1 = HttpContext.Current.Request.Cookies[".AspNet.Cookies"];
                if (cookie1 == null)
                {
                    string url = new UrlHelper(filterContext.HttpContext.Request.RequestContext).Action("Logout", "Account");
                    filterContext.Result = new RedirectResult(url);
                }
            }
            //else
            //{
            //    filterContext.Result = new RedirectResult(new UrlHelper(filterContext.HttpContext.Request.RequestContext).Action("Index", "User"));
            //}
            //if(memberBaseInfo  == null)
            //{
            //    string url = new UrlHelper(filterContext.HttpContext.Request.RequestContext).Action("DoLogin", "Home");
            //    filterContext.Result = new RedirectResult(url);
            //}
            //else
            //{
            //    if (controllerName == "Home" && actionName == "Index")
            //    {
            //        var cookieCheckLogin = filterContext.HttpContext.Request.Cookies["_CookieCheckLogin"];
            //        var cookie1 = HttpContext.Current.Request.Cookies[".AspNet.Cookies"];
            //        if (value != null)
            //        {
            //            filterContext.Result = null;
            //        }
            //        else
            //        {

            //            var cookie = filterContext.HttpContext.Request.Cookies[".AspNet.Cookies"];
            //            if (cookie == null)
            //            {
            //                filterContext.HttpContext.Request.GetOwinContext().Authentication.User =
            //               new GenericPrincipal(new GenericIdentity(string.Empty), null);
            //                if (!filterContext.HttpContext.Request.IsAuthenticated)
            //                {
            //                    filterContext.Result = null;
            //                }
            //            }

            //            else
            //            {
            //                if (!filterContext.HttpContext.Request.IsAuthenticated)
            //                {
            //                    filterContext.Result = null;
            //                }
            //                else
            //                {
            //                    filterContext.Result = new RedirectResult("Account/Dogin");
            //                }

            //            }
            //        }

            //    }
            //}

            //if (memberBaseInfo == null) {
            //    SignOutLocal(filterContext.HttpContext);
            //    string url = new UrlHelper(filterContext.HttpContext.Request.RequestContext).Action("Login", "Authen") ;

            //    //if (filterContext.HttpContext.Request.IsAjaxRequest() && filterContext.HttpContext.Request.HttpMethod == "POST")
            //    //{
            //    //    url = new UrlHelper(filterContext.HttpContext.Request.RequestContext).Action("Login", "Authen");
            //    //    filterContext.Result = new JsonResult
            //    //    {
            //    //        Data = new { redirect = url, status = 401 },
            //    //        JsonRequestBehavior = JsonRequestBehavior.AllowGet
            //    //    };
            //    //}
            //    //else if (controllerName == "Home" && actionName == "Index" && filterContext.HttpContext.Request.HttpMethod == "POST")
            //    //{
            //    //    filterContext.Result = new RedirectResult("Account/Login");
            //    //}
            //    //else
            //    //{
            //    //    filterContext.Result = new RedirectResult(url);
            //    //}
            //}


        }

        public static UserData CurrentUser(string value)
        {
            UserData userApp = null;
            var LoginTokenCookie = value;
            if (LoginTokenCookie != null)
            {
                try
                {
                    var token = FormsAuthentication.Decrypt(LoginTokenCookie);
                    userApp = JsonConvert.DeserializeObject<UserData>(token.UserData);
                    if (userApp == null )
                    {
                       
                        return null;
                    }
                }
                catch (Exception ex)
                {
                  
                }

            }
            return userApp;
        }
        private  void SignOutLocal(HttpContextBase httpContext)
        {
            

            FormsAuthentication.SignOut();
            httpContext.Session.Abandon();
            HttpContext.Current.Session.Abandon();
            //RemoveUserModel(httpContext);
        }

    }
}