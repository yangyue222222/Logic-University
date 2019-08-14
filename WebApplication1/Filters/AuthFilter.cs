using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WebApplication1.DAOs;
using WebApplication1.Models;
using WebApplication1.Utilities;
using System.Diagnostics;
using System.Web.Mvc.Filters;

namespace WebApplication1.Filters
{
    public class AuthFilter : ActionFilterAttribute,IAuthenticationFilter
    {
      
        void IAuthenticationFilter.OnAuthentication(AuthenticationContext filterContext)
        {
            if (filterContext.HttpContext.Request.Cookies["token"] != null)
            {
                string token = filterContext.HttpContext.Request.Cookies["token"].Value;
                string decodedToken = TokenUtility.Decrypt(token);
                string[] arr = decodedToken.Split(new string[] { "%" }, StringSplitOptions.None);
                int departmentId = Convert.ToInt32(arr[2]);

                Department d = new Department()
                {
                    DepartmentId = departmentId
                };
                User user = new User()
                {
                    Department = d,
                    UserId = Convert.ToInt32(arr[0])
                };

                User loggedInUser = UserDao.GetUserProfile(user);

                Debug.WriteLine("Auth Filter Department Id " + loggedInUser.Department.DepartmentId);

                if (loggedInUser != null)
                {
                    Controller controller = filterContext.Controller as Controller;
                    controller.ViewData["rank"] = loggedInUser.Rank; 
                    filterContext.RouteData.Values.Add("userId", loggedInUser.UserId);
                    filterContext.RouteData.Values.Add("departmentId", loggedInUser.Department.DepartmentId);
                    filterContext.RouteData.Values.Add("rank", loggedInUser.Rank);
                    filterContext.Controller.ViewData["Username"] = loggedInUser.Name;
                }
                else
                {
                    HttpCookie cookie = new HttpCookie("token", "...");
                    cookie.Expires = DateTime.Now.AddDays(-1);
                    filterContext.HttpContext.Response.Cookies.Add(cookie);

                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary
                        {
                            { "controller", "Auth" },
                            { "action", "Index" }
                        });
                }
            }
            else
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "controller", "Auth" },
                        { "action", "Index" }
                    });
            }



        }

        void IAuthenticationFilter.OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {
            
        }
    }
}