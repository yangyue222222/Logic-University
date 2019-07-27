using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using WebApplication1.DataAccessLayer;
using WebApplication1.Models;
using System.Web.Script.Serialization;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            using (var db = new UniDBContext())
            {
                var department = new Department()
                {
                    DepartmentName = "English",
                    PickupPoint = "ISS",
                };
                var user = new User()
                {
                    Name = "KKK",
                    Password = "ABCD",
                    Rank = 1,
                    Username = "AAA",
                    Department = department

                };

                db.Users.Add(user);
                db.SaveChanges();
            }


            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        [HttpGet]
        public FileResult Static(string filename)
        {
            Debug.WriteLine(filename);

            return File(Url.Content("~/Content/" + filename), "");
        }
    }
}