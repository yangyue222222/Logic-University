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
            
            ViewData["Disbursement"] = d;
            return View("DeliveryDetail");
        }

        [HttpPost, Route("deliveries/{id}")]
        public ActionResult ReceiveItemsByDepartment(List<DisbursementDetail> details,int id)
        {

            Dictionary<int,DisbursementDetail> dDict = DisbursementDao.GetDisbursementDetailDictByDisbursementId(id);

            //generate adjustment if items are missing
            List<AdjustmentDetail> adjustmentDetails = new List<AdjustmentDetail>();

            foreach (var d in details)
            {
                DisbursementDetail disDetail = dDict[d.DisbursementDetailId];
                if(disDetail.Quantity != d.Quantity && d.Quantity < disDetail.Quantity)
                {
                    AdjustmentDetail adDetail = new AdjustmentDetail()
                    {
                        Item = new Item()
                        {
                            ItemId = d.Item.ItemId
                        },
                        Count = disDetail.Quantity - d.Quantity
                        
                    };

                    adjustmentDetails.Add(adDetail);
                }
            }

            if(adjustmentDetails.Count > 0)
            {
                int userId = Convert.ToInt32(RouteData.Values["userId"]);
                User u = new User()
                {
                    UserId = userId
                };
                AdjustmentDao.InsertAdjustment(adjustmentDetails, u);
            }


            Disbursement dis = DisbursementDao.DeliverDisbursement(id, details);
            //need to update the request such as status, delivered Qty
            RequestDao.UpdateRequestById(dis.Request.RequestId);

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

            List<AdjustmentDetail> adjustmentDetails = new List<AdjustmentDetail>();
            bool valid = true;
            foreach(var i in items)
            {
                if (i.AllocatedQuantity > i.ActualQuantity)
                {
                    AdjustmentDetail detail = new AdjustmentDetail()
                    {
                        Count = (i.AllocatedQuantity - i.ActualQuantity),
                        Item = new Item()
                        {
                            ItemId = i.ItemId
                        }
                    };

                    adjustmentDetails.Add(detail);
                }else if (i.AllocatedQuantity == i.ActualQuantity)
                {

                }else 
                {
                    valid = false;
                    break;
                }
            }
            if(valid != false)
            {
                if (adjustmentDetails.Count > 0)
                {
                    int userId = Convert.ToInt32(RouteData.Values["userId"]);
                    User u = new User()
                    {
                        UserId = userId
                    };
                    AdjustmentDao.InsertAdjustment(adjustmentDetails, u);
                    ItemDao.UpdateStockForAdjustment(adjustmentDetails);
                }

                DisbursementDao.GenerateDisbursements(items);


            }
            else
            {
                return RedirectToAction("Retrieval", "Stationery");
            }
            



            //List<AdjustmentDetail> details = new List<AdjustmentDetail>();
            //foreach (var i in items)
            //{
            //    Debug.WriteLine(i.ItemId + " ------- " + i.ActualQuantity);
            //    if (i.ActualQuantity < i.AllocatedQuantity)
            //    {
            //        Item item = new Item()
            //        {
            //            ItemId = i.ItemId
            //        };
            //        AdjustmentDetail detail = new AdjustmentDetail()
            //        {
            //            Item = item,
            //            Count = i.AllocatedQuantity - i.ActualQuantity
            //        };

            //        details.Add(detail);
            //    }
            //}

            //if(details.Count > 0)
            //{
            //    Debug.WriteLine("Passed details count");
            //    int userId = Convert.ToInt32(RouteData.Values["userId"]);
            //    User u = new User()
            //    {
            //        UserId = userId
            //    };
            //    details = AdjustmentDao.InsertAdjustment(details, u);
            //    DisbursementDao.UpdateDisbursementDetailsForItemMissing(details);
            //}else
            //{
            //    DisbursementDao.UpdateDisbursementsStatus();
            //}

            return RedirectToAction("");
        }

        
    }
}