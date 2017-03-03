using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BlockChainWebApplication.Core.Utilities
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class AuthorizeAccessAttribute : AuthorizeAttribute
    {
        public bool RequireSuperAdmin { get; set; }

        private bool IsNoPermission { get; set; }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (AppManager.User == null)
            {
                return false;
            }
            else if (AppManager.User.IsSuperAdmin)
            {
                return true;
            }
            else if (RequireSuperAdmin)
            {
                IsNoPermission = true;
                return AppManager.User.IsSuperAdmin;
            }
            else
            {
                return true;
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = this.IsNoPermission ? (new AuthorizationController()).RedirectToDashBoard() : (new AuthorizationController()).RedirectToLogin();
        }
    }

    public sealed class AuthorizationController : Controller
    {

        public ActionResult RedirectToLogin()
        {
            return Redirect("/default");
        }

        public ActionResult RedirectToDashBoard()
        {
            return Redirect("/pv");
        }
    }
}