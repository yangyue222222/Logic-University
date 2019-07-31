using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using System.Diagnostics;
using WebApplication1.DAOs;
using WebApplication1.Utilities;
using WebApplication1.Filters;

namespace WebApplication1.Controllers
{
    [AuthFilter]
    public class RequisitionController : Controller
    {
        [HttpPost]
        public ActionResult Index(List<Item> items)
        {
            if (items != null)
            {
                foreach (var v in items)
                {
                    if (v.ItemId == 0 || v.Quantity < 1)
                    {
                        return new HttpStatusCodeResult(400);
                    }
                }

                try
                {
                    int departmentId = Convert.ToInt32(RouteData.Values["departmentId"]);
                    int userId = Convert.ToInt32(RouteData.Values["userId"]);
                    
                    
                    Department d = new Department()
                    {
                        DepartmentId = Convert.ToInt32(departmentId)
                    };
                    User u = new User()
                    {
                        UserId = userId,
                        Department = d
                    };
                    RequestDao.InsertRequest(items, u);
                    return new HttpStatusCodeResult(200);
                }
                catch (Exception e)
                {
                    return new HttpStatusCodeResult(400);
                }

            }
            else
            {
                return new HttpStatusCodeResult(400);
            }
            
            
        }
        [HttpGet]
        public ActionResult Index()
        {
            ItemDao itemDao = new ItemDao();
            Dictionary<string, List < Item >> items = itemDao.getItemsForRequisition();

            ViewData["Items"] = items;
            return View();
        }

        [HttpGet]
        public ActionResult PendingRequisitions()
        {

            string token = HttpContext.Request.Cookies["token"].Value;
            string decodedToken = TokenUtility.Decrypt(token);
            string[] arr = decodedToken.Split(new string[] { "%" }, StringSplitOptions.None);
            int departmentId = Convert.ToInt32(arr[2]);
            List<Request> requests = RequestDao.getRequestsByDepartment(departmentId);

            ViewData["Requests"] = requests;

            return View("PendingRequisitions");
        }

        [Route("Requisitions/{id}"),HttpGet]
        public JsonResult RequisitionById(int id)
        {
            int departmentId = Convert.ToInt32(RouteData.Values["departmentId"]);

            List<RequestDetail> requestDetails = RequestDao.getRequestDetailsByRequestId(id,departmentId);
            return Json(requestDetails,JsonRequestBehavior.AllowGet);
        }

        [Route("Requisitions/{id}"), HttpPost]
        public ActionResult ApproveRequisition(int id,string status)
        {
            int departmentId = Convert.ToInt32(RouteData.Values["departmentId"]);
            int userId = Convert.ToInt32(RouteData.Values["userId"]);

            Department d = new Department()
            {
                DepartmentId = departmentId
            };
            User u = new User()
            {
                UserId = userId
            };
            Request request = new Request()
            {
                ApprovedBy = u,
                RequestId = id,
                Department = d
            };
            if (status.Equals("ACCEPT"))
            {
                request.Status = (int)RequestStatus.Approved;
            }else
            {
                request.Status = (int)RequestStatus.Rejected;
            }

            RequestDao.updateRequestStatus(request);
            return RedirectToAction("PendingRequisitions");
        }

        
    }

}