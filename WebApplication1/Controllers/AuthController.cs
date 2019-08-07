using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using System.Diagnostics;
using WebApplication1.Utilities;
using WebApplication1.DAOs;
using WebApplication1.Filters;

namespace WebApplication1.App_Start
{
    public class AuthController : Controller
    {
        [HttpGet,Route("login")]
        public ActionResult Index()
        {
            return View("Login");
        }
        [HttpPost, Route]
        public ActionResult Login(User user)
        {
            //fetch from database
            if (ModelState.IsValid)
            {
                User u = UserDao.GetUser(user);
                if (u == null)
                {
                    ViewData["Error"] = "Username or Password is wrong.";
                    return View();
                }

                Debug.WriteLine(u.Department.DepartmentId);
                Debug.WriteLine(u.Department.DepartmentName);

                string token = TokenUtility.Encrypt(u);
                //HttpCookie cookie = new HttpCookie("token",token);
                //cookie.Expires = DateTime.Now.AddDays(1);
                //Response.Cookies.Add(cookie);
                Response.Cookies["token"].Value = token;
                
                RedirectToRouteResult result = null;
                switch (u.Rank)
                {
                    case (int)UserRank.Manager:
                        result = RedirectToAction("Dashboard");
                        break;

                    case (int)UserRank.Employee:
                        result = RedirectToAction("Index","Requisition");
                        break;

                    case (int)UserRank.Head:
                        result = RedirectToAction("PendingRequisitions", "Requisition");
                        break;
                    case (int)UserRank.Clerk:
                        result = RedirectToAction("Index", "Orders");
                        break;
                }

                return result;
            }
           
            return View();
        }
        [HttpPost]
        public ActionResult LoginMobile(User user)
        {
            //fetch from database
            if (ModelState.IsValid)
            {
                User u = UserDao.GetUser(user);
                if (u == null)
                {
                    ViewData["Error"] = "Username or Password is wrong.";
                    return View();
                }

                Debug.WriteLine(u.Department.DepartmentId);
                Debug.WriteLine(u.Department.DepartmentName);

                string token = TokenUtility.Encrypt(u);
                Response.Cookies["token"].Value = token;

                return Json(token,JsonRequestBehavior.AllowGet);
            }
            return Json("test", JsonRequestBehavior.AllowGet);
        }

        [HttpGet,Route("logout")]
        public ActionResult Logout()
        {
            if (HttpContext.Request.Cookies["token"] != null)
            {
                HttpCookie cookie = new HttpCookie("token","...");
                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookie);
            }
            return RedirectToAction("Index");
        }

        [AuthFilter]
        public ActionResult Dashboard()
        {
            return View();
        }
    }
}