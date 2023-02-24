using System.Web.Mvc;
using System.Web.Routing;

namespace EForm
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "ErrorHandler",
                url: "error",
                defaults: new { controller = "Authen", action = "ErrorHandler", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
            //routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapRoute(
            //    name: "RedirectLogin",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Authen", action = "RedirectLogin", id = UrlParameter.Optional }
            //);
        }
    }
}
