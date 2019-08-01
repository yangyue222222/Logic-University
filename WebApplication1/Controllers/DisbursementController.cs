using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.DAOs;
using WebApplication1.DataAccessLayer;
using System.Diagnostics;
using WebApplication1.Filters;

namespace WebApplication1.Controllers
{
    [AuthFilter]
    public class DisbursementController : Controller
    {

        [HttpGet,Route("deliveries/{id}")]
        public ActionResult Disbursements(int id)
        {
            Disbursement d = DisbursementDao.GetDisbursement(id);
            return View();
        }

        [HttpGet,Route("deliveries")]
        public ActionResult Deliveries()
        {
            List<Disbursement> disbursements = DisbursementDao.GetPreparedDisbursements();
            ViewData["Disbursements"] = disbursements;
            return View("Deliveries");
        }




        //retrieving and preparing for delivery

        [HttpPost]
        public ActionResult Disbursements(List<RetrievalItem> items)
        {
            List<AdjustmentDetail> details = new List<AdjustmentDetail>();
            foreach (var i in items)
            {
                Debug.WriteLine(i.ItemId + " ------- " + i.ActualQuantity);
                if (i.ActualQuantity < i.AllocatedQuantity)
                {
                    Item item = new Item()
                    {
                        ItemId = i.ItemId
                    };
                    AdjustmentDetail detail = new AdjustmentDetail()
                    {
                        Item = item,
                        Count = i.AllocatedQuantity - i.ActualQuantity
                    };

                    details.Add(detail);
                }
            }

            if(details.Count > 0)
            {
                Debug.WriteLine("Passed details count");
                int userId = Convert.ToInt32(RouteData.Values["userId"]);
                User u = new User()
                {
                    UserId = userId
                };
                details = AdjustmentDao.InsertAdjustment(details, u);
                DisbursementDao.UpdateDisbursementDetailsForItemMissing(details);
            }else
            {
                DisbursementDao.UpdateDisbursementsStatus();
            }

            return RedirectToAction("");
        }

        
    }
}