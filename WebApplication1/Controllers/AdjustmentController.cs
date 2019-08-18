using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.DAOs;
using WebApplication1.Filters;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [AuthFilter]
    public class AdjustmentController : Controller
    {
        [HttpGet,Route("makeadjustments",Name = "makeadjustments")]
        [AuthorizeFilter((int)UserRank.Manager,(int)UserRank.Supervisor,(int)UserRank.Clerk)]
        public ActionResult MakeAdjustments()
        {
            Dictionary<string, List<Item>> items = ItemDao.getItemsForRequisition();
            ViewData["Items"] = items;
            return View("MakeAdjustment");
        }

        [HttpPost,Route("adjustments")]
        [AuthorizeFilter((int)UserRank.Manager, (int)UserRank.Supervisor, (int)UserRank.Clerk)]
        public ActionResult Adjustments(List<Item> items)
        {

            int userId = Convert.ToInt32(RouteData.Values["userId"]);
            User u = new User()
            {
                UserId = userId
            };

            List<AdjustmentDetail> details = new List<AdjustmentDetail>();
            foreach(var i in items)
            {
                if(i.Quantity > 0)
                {
                    AdjustmentDetail adjDetail = new AdjustmentDetail()
                    {
                        Item = new Item()
                        {
                            ItemId = i.ItemId
                        },
                        Count = i.Quantity
                    };

                    details.Add(adjDetail);
                }
                
            }

            if(details.Count > 0)
            {
                foreach(var d in details)
                {
                    Debug.WriteLine("Item Id {0} Amount is {1} Requestor is {2}",d.Item.ItemId,d.Count,u.UserId);

                }

                Adjustment ad = AdjustmentDao.InsertAdjustment(details, u);
                AdjustmentDao.CalculateAdjustmentCost(ad);
                ItemDao.UpdateStockForAdjustment(details);
            }

            return new HttpStatusCodeResult(200);
        }

        [HttpGet, Route("adjustments", Name = "adjustments")]
        [AuthorizeFilter((int)UserRank.Manager, (int)UserRank.Supervisor)]
        public ActionResult GetAllAdjustments()
        {
            int rank = Convert.ToInt32(RouteData.Values["rank"]);
            List<Adjustment> adjustments = AdjustmentDao.GetAllAdjustments(rank);
            ViewData["Adjustments"] = adjustments;
            return View("Adjustments");
        }

        [HttpGet, Route("adjustments/{adjustmentId}")]
        [AuthorizeFilter((int)UserRank.Manager, (int)UserRank.Supervisor)]
        public ActionResult GetAdjustmentById(int adjustmentId)
        {

            Adjustment adj = AdjustmentDao.GetAdjustmentDetailById(adjustmentId);
            ViewData["Adjustment"] = adj;
            ViewData["MyAdjustment"] = 1;
            return View("AdjustmentDetail");
        }

        [HttpPost, Route("adjustments/{adjustmentId}")]
        [AuthorizeFilter((int)UserRank.Manager, (int)UserRank.Supervisor)]
        public ActionResult UpdateAdjustmentStatus(int adjustmentId)
        {
            int userId = Convert.ToInt32(RouteData.Values["userId"]);
            User u = new User()
            {
                UserId = userId
            };
            AdjustmentDao.UpdateAdjustmentStatus(adjustmentId,u);
            return RedirectToAction("GetPendingAdjustments");
        }

        [HttpGet, Route("myadjustments",Name = "myadjustments")]
        [AuthorizeFilter((int)UserRank.Manager, (int)UserRank.Supervisor,(int)UserRank.Clerk)]
        public ActionResult GetAdjustmentsByClerk()
        {
            int rank = (int)UserRank.Clerk;
            int userId = Convert.ToInt32(RouteData.Values["userId"]);
            int departmentId = Convert.ToInt32(RouteData.Values["departmentId"]);

            List<Adjustment> adjustments = AdjustmentDao.GetAllAdjustmentsByClerk(userId, departmentId);
            ViewData["Adjustments"] = adjustments;
            //reuse the view for storemanager and storesupervisor
            ViewData["MyAdjustment"] = 1;
            return View("Adjustments");
        }

        [HttpGet, Route("myadjustments/{adjustmentId}")]
        [AuthorizeFilter((int)UserRank.Manager, (int)UserRank.Supervisor, (int)UserRank.Clerk)]
        public ActionResult GetMyAdjustmentDetailById(int adjustmentId)
        {

            Adjustment adjustment = AdjustmentDao.GetAdjustmentDetailById(adjustmentId);
            ViewData["Adjustment"] = adjustment;
            //reuse the view for storemanager and storesupervisor
            
            return View("AdjustmentDetail");
        }

        [HttpGet, Route("pendingadjustments", Name = "pendingadjustments")]
        [AuthorizeFilter( (int)UserRank.Manager,(int)UserRank.Supervisor)]
        public ActionResult GetPendingAdjustments()
        {
            int userId = Convert.ToInt32(RouteData.Values["userId"]);
            List<Adjustment> adjustments = AdjustmentDao.GetAllPendingAdjustments(userId);
            ViewData["Adjustments"] = adjustments;
            return View("PendingAdjustments");
        }
    }
}