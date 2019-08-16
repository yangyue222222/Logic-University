﻿using System;
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
        [Route("~/")]
        public ActionResult Index()
        {
            if (Request.Cookies["token"] != null)
            {
                string token = Request.Cookies["token"].Value;
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
                if(loggedInUser != null)
                {
                    RedirectToRouteResult result = null;
                    switch (loggedInUser.Rank)
                    {
                        case (int)UserRank.Manager:
                            result = RedirectToRoute("pendingorders");
                            break;

                        case (int)UserRank.Supervisor:
                            result = RedirectToRoute("pendingorders");
                            break;

                        case (int)UserRank.TemporaryHead:
                            result = RedirectToRoute("requestitems");
                            break;

                        case (int)UserRank.Employee:
                            result = RedirectToRoute("requestitems");
                            break;

                        case (int)UserRank.Head:
                            result = RedirectToRoute("pendingrequisitions");
                            break;

                        case (int)UserRank.Clerk:
                            result = RedirectToRoute("orderitems");
                            break;
                    }

                    return result;
                }
                else
                {
                    HttpCookie cookie = new HttpCookie("token", "...");
                    cookie.Expires = DateTime.Now.AddDays(-1);
                    Response.Cookies.Add(cookie);
                    return RedirectToAction("Index");
                }

            }
            else
            {
                return View("Login");
            }


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

                string token = TokenUtility.Encrypt(u);
                HttpCookie cookie = new HttpCookie("token",token);
                cookie.Expires = DateTime.Now.AddDays(1);
                Response.Cookies.Add(cookie);
                
                RedirectToRouteResult result = null;
                switch (u.Rank)
                {
                    case (int)UserRank.Manager:
                        result = RedirectToRoute("PendingOrders");
                        break;

                    case (int)UserRank.Supervisor:
                        result = RedirectToRoute("PendingOrders");
                        break;

                    case (int)UserRank.TemporaryHead:
                        result = RedirectToRoute("requestitems");
                        break;

                    case (int)UserRank.Employee:
                        result = RedirectToRoute("requestitems");
                        break;

                    case (int)UserRank.Head:
                        result = RedirectToRoute("pendingrequisitions");
                        break;
                    case (int)UserRank.TemporaryHead:
                        result = RedirectToAction("PendingRequisitions", "Requisition");
                        break;
                    case (int)UserRank.Clerk:
                        result = RedirectToRoute("orderitems");
                        break;
                }


                return result;
            }
           
            return View();
        }
        [HttpPost,Route("loginmobile")]
        public ActionResult LoginMobile(User user)
        {
            var failLogin = new
            {
                Rank = 99,
            };
            //fetch from database
            if (ModelState.IsValid)
            {
                User u = UserDao.GetUser(user);
                if (u == null)
                {
                    return Json(failLogin, JsonRequestBehavior.AllowGet);
                }

                Debug.WriteLine(u.Department.DepartmentId);
                Debug.WriteLine(u.Department.DepartmentName);

                string token = TokenUtility.Encrypt(u);
                Response.Cookies["token"].Value = token;
                Response.Cookies["token"].Expires = DateTime.Now.AddDays(1);

                var login = new
                {
                    Department = u.Department.DepartmentId,
                    UserId = u.UserId,
                    Name = u.Name,
                    Rank = u.Rank,
                };
                return Json(login,JsonRequestBehavior.AllowGet);
            }
            return Json(failLogin, JsonRequestBehavior.AllowGet);
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
        [HttpGet,Route("logoutmobile")]
        public ActionResult LogoutMobile()
        {
            if (HttpContext.Request.Cookies["token"] != null)
            {
                Response.Cookies["token"].Expires = DateTime.Now.AddDays(-1);
            }
            var logout = new { status = 1 };
            return Json(logout, JsonRequestBehavior.AllowGet);
        }

        [AuthFilter]
        public ActionResult Dashboard()
        {
            return View();
        }
    }
}