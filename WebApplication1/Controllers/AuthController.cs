using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using System.Diagnostics;
using WebApplication1.Utilities;
using System.Diagnostics;
using WebApplication1.DAOs;

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
                HttpCookie cookie = new HttpCookie("token",token);
                cookie.Expires = DateTime.Now.AddDays(1);
                Response.Cookies.Add(cookie);
                return RedirectToAction("Index", "Requisition");
            }
           
            return View();
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
    }
}