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

        //for employee to get delivered disbursement and approve it
        [HttpGet,Route("delivereddisbursements",Name = "delivereddisbursements")]
        public ActionResult GetDeliveredDisbursements()
        {
            int departmentId = Convert.ToInt32(RouteData.Values["departmentId"]);
            List<Disbursement> disbursements = DisbursementDao.GetDisbursementsByDepartmentAndMonth(departmentId, DateTime.Now.Month,(int)DisbursementStatus.Delivered);
            ViewData["Disbursements"] = disbursements;
            return View("DeliveredDisbursements");
        }

        [HttpGet, Route("delivereddisbursements/{disbursementId}")]
        public ActionResult GetDeliveredDisbursementDetailById(int disbursementId)
        {
            int departmentId = Convert.ToInt32(RouteData.Values["departmentId"]);
            Disbursement d = DisbursementDao.GetDeliveredDisbursementById(disbursementId);
            ViewData["Disbursement"] = d;
            return View("DisbursementDetail");
        }


        //approve disbursement by employee
        [HttpGet,Route("approvedisbursements/{disbursementId}")]
        public ActionResult ApproveDisbursementById(int disbursementId)
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

            u.Department = d;

            DisbursementDao.ApproveDisubrsementById(u, disbursementId);

            return RedirectToAction("GetDeliveredDisbursements");

        }



        //when reach to the collection point and storeman will click received to the particular disbursement id
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
                Adjustment ad = AdjustmentDao.InsertAdjustment(adjustmentDetails, u);
                AdjustmentDao.CalculateAdjustmentCost(ad);
            }


            Disbursement dis = DisbursementDao.DeliverDisbursement(id, details);
            //need to update the request such as status, delivered Qty
            RequestDao.UpdateRequestById(dis.Request.RequestId);

            return RedirectToAction("Deliveries");
        }


        //storeman will get all of the prepared disbursements
        [HttpGet,Route("deliveries",Name = "deliveries")]
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
                        Count = (i.StockQuantity - i.ActualQuantity),
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
                    Adjustment ad = AdjustmentDao.InsertAdjustment(adjustmentDetails, u);
                    AdjustmentDao.CalculateAdjustmentCost(ad);
                    ItemDao.UpdateStockForAdjustment(adjustmentDetails);
                }

                DisbursementDao.GenerateDisbursements(items);
                return RedirectToAction("Retrieval", "Stationery");
            }
            else
            {
                return RedirectToAction("Retrieval", "Stationery");
            }

        }

        [HttpGet,Route("invoices",Name = "invoices")]
        public ActionResult GenerateInvoices()
        {
            List<Department> departments = DepartmentDao.GetAllDepartments();
            
            Dictionary<int, string> monthDict = new Dictionary<int, string>();
            foreach( var i in Enum.GetValues(typeof(Months))){
                monthDict.Add((int)i, i.ToString());
            }
            ViewData["Departments"] = departments;
            ViewData["MonthDict"] = monthDict;
            return View("Invoices");
        }

        [HttpGet,Route("disbursements")]
        public ActionResult GetDisbursementListWithMonth(int departmentId,int month)
        {
            List<Disbursement> disbursements = DisbursementDao.GetDisbursementsByDepartmentAndMonth(departmentId, month,(int)DisbursementStatus.Approved);
            List<object> data = new List<object>();
            if(disbursements != null)
            {
                foreach(var d in disbursements)
                {
                    data.Add(new
                    {
                        DisbursementId = d.DisbursementId,
                        Total = d.DisbursementDetails.Sum(dd => dd.Item.Price * dd.Quantity )
                });
                }
                
            }

            return Json(new {results = data },JsonRequestBehavior.AllowGet);
        }

        [HttpGet,Route("generateinvoice")]
        public ActionResult GenerateInvoice(int departmentId,int month) {
            try
            {
                DisbursementDao.GenerateInvoiceByDepartmentAndMonth(departmentId, month);
                return RedirectToAction("GenerateInvoices");
            }
            catch (Exception e)
            {
                return RedirectToAction("GenerateInvoices");
            }
        }

        //[HttpGet,Route("delivereddisbursements")]
        //public ActionResult GetDeliveredDisbursementById(int disbursementId)
        //{
        //    Disbursement d = DisbursementDao.GetDeliveredDisbursementById(disbursementId);
        //    ViewData["Disbursement"] = d;
        //    return View("DisbursementDetail");
        //}

        
    }
}