using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebCar.Filters
{
    public class AuthorizeRoleAttribute : AuthorizeAttribute
    {
        private readonly string[] _allowedRoles;

        public AuthorizeRoleAttribute(params string[] roles)
        {
            _allowedRoles = roles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            // Check if user is authenticated
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }

            // Check session
            if (httpContext.Session["CustomerId"] == null)
            {
                return false;
            }

            // Get user role from session
            var userRole = httpContext.Session["RoleName"]?.ToString();

            if (string.IsNullOrEmpty(userRole))
            {
                return false;
            }

            // Check if user role is in allowed roles
            return _allowedRoles.Contains(userRole, StringComparer.OrdinalIgnoreCase);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                // ✅ FIX: User is logged in but doesn't have permission
                filterContext.Result = new ViewResult
                {
                    ViewName = "~/Views/Account/AccessDenied.cshtml"
                };
            }
            else
            {
                // User is not logged in
                filterContext.Result = new RedirectResult("~/Account/Login? returnUrl=" +
                    HttpUtility.UrlEncode(filterContext.HttpContext.Request.RawUrl));
            }
        }
    }
}