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
using WebApplication1.Filters;

namespace WebApplication1.Controllers
{
    [AuthFilter]
    [AuthorizeFilter((int)UserRank.Head)]
    public class DelegateController : Controller
    {
        // GET: Delegate
        [Route("Delegate",Name = "Delegate"),HttpGet]
        public async Task<ActionResult> Delegate()
        {
            int departmentId = Convert.ToInt32(RouteData.Values["departmentId"]);

            Task<User> representativeTask = UserDao.GetRepresentative(departmentId);
            Task<List<User>> allEmployeesTask = UserDao.GetAllEmployeesFromDepartment(departmentId);
            Task<User> temporaryHeadTask = UserDao.GetTemporaryHeadByDepartment(departmentId);
            Task<List<PickUpPoint>> pickUpPointsTask = PickUpPointDao.GetAllPickupPoints();
            Task<PickUpPoint> getCurrentPickupTask = PickUpPointDao.GetPickupPointByDepartment(departmentId);

            User user = await representativeTask;
            List<User> users = await allEmployeesTask ;
            User temporaryHead = await temporaryHeadTask;
            List<PickUpPoint> points = await pickUpPointsTask;
            PickUpPoint point = await getCurrentPickupTask;

            ViewData["Employees"] = users;
            ViewData["CurrentRepresentative"] = user;
            ViewData["TemporaryHead"] = temporaryHead;
            ViewData["PickUpPoints"] = points;
            ViewData["CurrentPickUpPoint"] = point;
            return View("Index");
        }

        [HttpPost]
        public ActionResult Representative(int representativeId)
        {
            int departmentId = Convert.ToInt32(RouteData.Values["departmentId"]);
            DepartmentDao.UpdateDepartmentRepresentative(representativeId,departmentId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult PickupPoint(int pickupPointId)
        {
            int departmentId = Convert.ToInt32(RouteData.Values["departmentId"]);
            DepartmentDao.UpdatePickUpPoint(departmentId, pickupPointId);
            return RedirectToAction("Index");
        }



        [HttpGet,Route("delegateauthority")]
        public ActionResult DelegateAuthority(int delegateAuthority, int userId) {
            //1 means assign
            int departmentId = Convert.ToInt32(RouteData.Values["departmentId"]);
            if (delegateAuthority == 1)
            {
                DepartmentDao.AssignTemporaryHead(departmentId, userId);
            }
            //2 means cancel
            else if (delegateAuthority == 2)
            {
                DepartmentDao.CancelTemporaryHead(departmentId, userId);
            }

            return RedirectToAction("Delegate");
        }
    }
}