using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebApplication1.Filters
{
    public class AuthorizeFilter : AuthorizeAttribute,IAuthorizationFilter
    {
        private readonly int[] allowedRoles;
        public AuthorizeFilter(params int[] allowedRoles)
        {
            this.allowedRoles = allowedRoles;
        }

        //protected override bool AuthorizeCore(HttpContextBase httpContext)
        //{
        //    bool authorize = false;
        //    int requestorRank = Convert.ToInt32(httpContext.Request.RequestContext.RouteData.Values["rank"]);
            

        //    return authorize;
        //}

        void IAuthorizationFilter.OnAuthorization(AuthorizationContext filterContext)
        {
            int requestorRank = Convert.ToInt32(filterContext.RequestContext.RouteData.Values["rank"]);
            bool authorize = false;
            foreach (var i in allowedRoles)
            {
                if (i == requestorRank)
                {
                    authorize = true;
                    break;
                }

            }


            if (authorize != true)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                            { "controller", "Auth" },
                            { "action", "Index" }
                    });
            }
        }
    }
}