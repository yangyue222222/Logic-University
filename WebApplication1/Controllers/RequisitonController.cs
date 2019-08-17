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
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    [AuthFilter]
    public class RequisitionController : Controller
    {

        [HttpGet, Route("requestitems", Name = "requestitems")]
        [AuthorizeFilter((int)UserRank.Head, (int)UserRank.TemporaryHead, (int)UserRank.Employee)]
        public ActionResult Index()
        {
            Dictionary<string, List<Item>> items = ItemDao.getItemsForRequisition();

            ViewData["Items"] = items;
            return View();
        }

        [HttpPost, Route("requestitems")]
        [AuthorizeFilter((int)UserRank.Head, (int)UserRank.TemporaryHead, (int)UserRank.Employee)]
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
                    Task.Run(() => EmailUtility.SendEmailForApproval(departmentId));
                    return new HttpStatusCodeResult(200);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    return new HttpStatusCodeResult(400);
                }

            }
            else
            {
                return new HttpStatusCodeResult(400);
            }


        }
        
        //dept head gets pending requisitions to approve
        [HttpGet,Route("pendingrequisitions",Name = "pendingrequisitions")]
        [AuthorizeFilter((int)UserRank.Head, (int)UserRank.TemporaryHead)]
        public ActionResult PendingRequisitions()
        {
            int departmentId = Convert.ToInt32(RouteData.Values["departmentId"]);

            List<Request> requests = RequestDao.getRequestsByDepartment(departmentId);

            ViewData["Requests"] = requests;

            return View("PendingRequisitions");
        }

        //get single pending requisition
        [Route("Requisitions/{id}"), HttpGet]
        [AuthorizeFilter((int)UserRank.Head, (int)UserRank.TemporaryHead, (int)UserRank.Employee)]
        public JsonResult RequisitionById(int id)
        {
            int departmentId = Convert.ToInt32(RouteData.Values["departmentId"]);

            List<RequestDetail> requestDetails = RequestDao.getRequestDetailsByRequestId(id, departmentId);
            return Json(requestDetails, JsonRequestBehavior.AllowGet);
        }

        //approve single requisition
        [Route("Requisitions/{id}"), HttpPost]
        [AuthorizeFilter((int)UserRank.Head, (int)UserRank.TemporaryHead)]
        public ActionResult ApproveRequisition(int id, string status)
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
            } else
            {
                request.Status = (int)RequestStatus.Rejected;
            }

            RequestDao.ApproveRequest(request);
          
            return RedirectToAction("PendingRequisitions");
        }

        //get own requisitions history
        [HttpGet,Route("myrequisitions",Name = "myrequisitions")]
        [AuthorizeFilter((int)UserRank.Head, (int)UserRank.TemporaryHead, (int)UserRank.Employee)]
        public ActionResult MyRequisitions()
        {
            int departmentId = Convert.ToInt32(RouteData.Values["departmentId"]);
            int userId = Convert.ToInt32(RouteData.Values["userId"]);

            User u = new User()
            {
                UserId = userId,
                Department = new Department()
                {
                    DepartmentId = departmentId
                }
            };
            List<Request> requests = RequestDao.GetMyRequests(u);

            ViewData["Requests"] = requests;

            return View();
        }

        //get detail of request by all roles
        [HttpGet,Route("myrequisitions/{requestId}")]
        [AuthorizeFilter((int)UserRank.Head, (int)UserRank.TemporaryHead, (int)UserRank.Employee,(int)UserRank.Manager,(int)UserRank.Supervisor,(int)UserRank.Clerk)]
        public ActionResult GetMyRequisition(int requestId)
        {
            int departmentId = Convert.ToInt32(RouteData.Values["departmentId"]);
            int userId = Convert.ToInt32(RouteData.Values["userId"]);
            int rank = Convert.ToInt32(RouteData.Values["rank"]);
            string layout = "";
            if (rank == (int)UserRank.Clerk)
            {
                layout = "_StoreLayout.cshtml";
            }else
            {
                layout = "_DeptLayout.cshtml";
            }
            User u = new User()
            {
                UserId = userId,
                Department = new Department()
                {
                    DepartmentId = departmentId
                },
                Rank = rank
            };
            Request r = RequestDao.GetRequestById(requestId, u);
            ViewData["Request"] = r;
            if (r != null)
            {
                ViewData["LayoutName"] = layout;
                return View("MyRequest");
            }
            return RedirectToAction("myrequisitions");
        }

        [HttpPost,Route("cancelrequisitions/{requestId}")]
        [AuthorizeFilter((int)UserRank.TemporaryHead, (int)UserRank.Employee,(int)UserRank.Head)]
        public ActionResult CancelMyRequisitions(int requestId)
        {
            int departmentId = Convert.ToInt32(RouteData.Values["departmentId"]);
            int userId = Convert.ToInt32(RouteData.Values["userId"]);

            User u = new User()
            {
                UserId = userId,
                Department = new Department()
                {
                    DepartmentId = departmentId
                }
            };

            RequestDao.CancelRequestById(requestId, u);

            return RedirectToAction("MyRequisitions");
        }

        [HttpGet, Route("outstandingrequests", Name = "outstandingrequests")]
        [AuthorizeFilter((int)UserRank.Manager, (int)UserRank.Supervisor, (int)UserRank.Clerk)]
        public ActionResult GetOutStandingRequest()
        {
            List<Request> requests = RequestDao.GetUnFulfilledRequests();
            ViewData["Requests"] = requests;
            return View("UnFulfilledRequests");
        }


        //YANG Part

        [HttpGet, Route("history", Name = "history")]
        public ActionResult GetHistoricalRequisitions()
        {
            int departmentId = Convert.ToInt32(RouteData.Values["departmentId"]);
            List<Request> history = RequestDao.getHistoricalRequestsByDepartment(departmentId);
            ViewData["History"] = history;
            return View("RequisitionHistory");
        }

        [Route("requisition/generaterequisitionreport")]
        public ActionResult GenerateRequisitionReport()
        {
            List<Department> departments = DepartmentDao.GetAllDepartments();

            Dictionary<int, string> monthDict = new Dictionary<int, string>();
            foreach (var i in Enum.GetValues(typeof(Months)))
            {
                monthDict.Add((int)i, i.ToString());
            }
            ViewData["Departments"] = departments;
            ViewData["MonthDict"] = monthDict;
            return View("ReqReport");
        }
        [HttpGet, Route("reqhistory")]
        public ActionResult GetRequisitionByMonth(int deptId, int month)
        {
            List<RetrievalItem> reqs = RequestDao.getRequestedItemsByMonth(deptId, month);

            return Json(new { results = reqs }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult PendingMobile()
        {
            int departmentId = Convert.ToInt32(RouteData.Values["departmentId"]);
            List<Request> requests = RequestDao.getRequestsByDepartment(departmentId);
            return Json(requests, JsonRequestBehavior.AllowGet);
        }


    }

}