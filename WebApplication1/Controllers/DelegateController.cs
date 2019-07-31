using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.DAOs;
using WebApplication1.Utilities;
using System.Diagnostics;

namespace WebApplication1.Controllers
{
    public class DelegateController : Controller
    {
        // GET: Delegate
        [Route("Delegate"),HttpGet]
        public async Task<ActionResult> Index()
        {
            string token = HttpContext.Request.Cookies["token"].Value;
            string decryptedToken = TokenUtility.Decrypt(token);
            string[] arr = decryptedToken.Split(new string[] { "%" }, StringSplitOptions.None);
            int departmentId = Convert.ToInt32(arr[2]);

            Task<User> representativeTask = UserDao.GetRepresentative(departmentId);
            Task<List<User>> allEmployeesTask = UserDao.GetAllEmployeesFromDepartment(departmentId);


            User user = await representativeTask;
            List<User> users = await allEmployeesTask ;

            ViewData["Employees"] = users;
            ViewData["CurrentRepresentative"] = user;

            return View();
        }

        [HttpPost]
        public ActionResult Representative(int representativeId)
        {
            string token = Request.Cookies["token"].Value;
            string decryptedToken = TokenUtility.Decrypt(token);
            string[] arr = decryptedToken.Split(new string[] { "%"},StringSplitOptions.None);
            int departmentId = Convert.ToInt32(arr[2]);

            DepartmentDao.UpdateDepartmentRepresentative(representativeId,departmentId);
            return RedirectToAction("Index");
        }
    }
}