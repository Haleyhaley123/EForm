using Microsoft.Owin;
using Owin;
using System.Web.Http;
using EForm.Middlewares;
using Microsoft.Owin.Security.Cookies;
using DataAccess.Repository;
using System.Collections.Generic;
using System;
using System.Configuration;

[assembly: OwinStartup(typeof(EForm.Startup))]

namespace EForm
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Enable CORS (cross origin resource sharing) for making request using browser from different domains
            if (ConfigurationManager.AppSettings["HiddenError"].Equals("false"))
                app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);

            //Cookie authenicate
            var session_timeout = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["SessionTimeout"]);
            app.UseCookieAuthentication(new CookieAuthenticationOptions()
            {
                AuthenticationType = "ApplicationCookie",
                CookieName = "EFormED",
                SlidingExpiration = true,
                ExpireTimeSpan = TimeSpan.FromMinutes(session_timeout),
                Provider = new CookieAuthenticationProvider
                {
                    OnResponseSignedIn = context =>
                    {
                        var cookies = context.Response.Headers.GetCommaSeparatedValues("Set-Cookie");
                        var cookieValue = GetAuthenCookie(cookies);

                        if (!string.IsNullOrEmpty(cookieValue))
                            UpdateSession(context.Identity.Name, cookieValue);
                    }
                }
            });
            //Log middleware
            app.Use(typeof(LogChangeMiddleware));

            HttpConfiguration config = new HttpConfiguration();
            WebApiConfig.Register(config);
        }

        private string GetAuthenCookie(IList<string> cookies)
        {
            var cookieValue = "";
            var cookieValue1 = "";
            var cookieValue2 = "";
            var cookieValue3 = "";
            foreach (var cookie in cookies)
            {
                var cookieKeyIndex = cookie.IndexOf("EFormED");
                var cookieKeyIndex1 = cookie.IndexOf("EFormEDC1");
                var cookieKeyIndex2 = cookie.IndexOf("EFormEDC2");
                if(cookies.Count > 1)
                {
                    if (cookieKeyIndex1 != -1)
                    {
                        cookieValue2 = cookie.Substring("EFormEDC1".Length + 1);
                    }
                    if (cookieKeyIndex2 != -1)
                    {
                        // Add extra character for '='
                        cookieValue3 = cookie.Substring("EFormEDC2".Length + 1);
                    }
                }
                else               
                if (cookieKeyIndex != -1)
                {
                    cookieValue1 = cookie.Substring("EFormED".Length + 1);
                }
                cookieValue = cookieValue1 + cookieValue2 + cookieValue3;
            }
            return cookieValue;
        }
        private void UpdateSession(string username, string cookieValue)
        {
            try
            {
                using (var unitOfWork = new EfUnitOfWork())
                {
                    var user = unitOfWork.UserRepository.FirstOrDefault(m => !m.IsDeleted && m.Username == username);
                    user.SessionId = cookieValue.Substring(0, 20);
                    user.Session = cookieValue;
                    unitOfWork.UserRepository.Update(user);
                    unitOfWork.Commit();
                }
            }
            catch (Exception) { }
        }
    }
}
