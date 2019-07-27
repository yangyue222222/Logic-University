using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.DAOs;
using WebApplication1.Models;
using System.Diagnostics;

namespace WebApplication1.Controllers
{
    public class OrderController : Controller
    {
        // GET: Order
        public ActionResult Index()
        {
            string token = HttpContext.Request.Cookies["token"].ToString();
            string[] arr = token.Split(new string[] { "%"},StringSplitOptions.None);
            int departmentId = Convert.ToInt32(arr[2]);
            List<Request> requests = RequestDao.getRequestsByDepartment(departmentId);
            Debug.WriteLine("Requests");
            foreach(var r in requests)
            {
                Debug.WriteLine(r.RequestId);
            }
            return View("OrderView");
        }
    }
}