using EForm.Helper;

using System.Web.Mvc;

namespace EForm.Authentication
{
    public class CheckLogin : FilterAttribute,IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            //if (!GetCurrentUser.CheckCurrentLogin())
            //{
            //    filterContext.Result = new RedirectResult("/Login");
            //}

            //if (SecureDongleProvider.CheckHardKey() == false)
            //{
            //    filterContext.Result = new RedirectResult("/Login");
            //}
        }

    }
}