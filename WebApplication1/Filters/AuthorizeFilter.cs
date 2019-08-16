using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WebApplication1.Models;

namespace WebApplication1.Filters
{
    public class AuthorizeFilter : AuthorizeAttribute,IAuthorizationFilter
    {
        private readonly int[] allowedRoles;
        public AuthorizeFilter(params int[] allowedRoles)
        {
            this.allowedRoles = allowedRoles;
        }

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
                RedirectToRouteResult result = null;
                switch (requestorRank)
                {
                    case (int)UserRank.Manager:
                        result =  new RedirectToRouteResult(
                                        new RouteValueDictionary
                                        {
                                            { "controller", "Orders" },
                                            { "action", "PendingOrders" }
                                        });
                        break;

                    case (int)UserRank.Supervisor:
                        result = new RedirectToRouteResult(
                                        new RouteValueDictionary
                                        {
                                            { "controller", "Orders" },
                                            { "action", "PendingOrders" }
                                        });
                        break;

                    case (int)UserRank.Employee:
                        result = new RedirectToRouteResult(
                                        new RouteValueDictionary
                                        {
                                            { "controller", "Requisition" },
                                            { "action", "Index" }
                                        });
                        break;

                    case (int)UserRank.TemporaryHead:
                        result = new RedirectToRouteResult(
                                        new RouteValueDictionary
                                        {
                                            { "controller", "Requisition" },
                                            { "action", "Index" }
                                        });
                        break;

                    case (int)UserRank.Head:
                        result = new RedirectToRouteResult(
                                        new RouteValueDictionary
                                        {
                                            { "controller", "Requisition" },
                                            { "action", "PendingRequisitions" }
                                        });
                        break;

                    case (int)UserRank.Clerk:
                        result = new RedirectToRouteResult(
                                        new RouteValueDictionary
                                        {
                                            { "controller", "Orders" },
                                            { "action", "Index" }
                                        });
                        break;
                }


                filterContext.Result = result;
            }
        }
    }
}