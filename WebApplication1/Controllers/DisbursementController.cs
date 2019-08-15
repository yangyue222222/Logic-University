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
        [AuthorizeFilter((int)UserRank.Manager, (int)UserRank.Supervisor, (int)UserRank.Clerk)]
        public ActionResult Disbursements(int id)
        {
            Disbursement d = DisbursementDao.GetDisbursement(id);
            
            ViewData["Disbursement"] = d;
            return View("DeliveryDetail");
        }

        //for representative employee to get delivered disbursement and approve it
        [HttpGet,Route("delivereddisbursements",Name = "delivereddisbursements")]
        [AuthorizeFilter((int)UserRank.Employee)]
        public ActionResult GetDeliveredDisbursements()
        {
            int departmentId = Convert.ToInt32(RouteData.Values["departmentId"]);
            int userId = Convert.ToInt32(RouteData.Values["userId"]);
            List<Disbursement> disbursements = DisbursementDao.GetDisbursementsByDepartmentAndMonth(userId,departmentId, DateTime.Now.Month,(int)DisbursementStatus.Delivered);
            ViewData["Disbursements"] = disbursements;
            return View("DeliveredDisbursements");
        }


        [HttpGet,Route("delivereddisbursements/{disbursementId}")]
        public ActionResult GetDeliveredDisbursementDetailById(int disbursementId)
        {
            int departmentId = Convert.ToInt32(RouteData.Values["departmentId"]);
            Disbursement d = DisbursementDao.GetDeliveredDisbursementById(disbursementId);
            ViewData["Disbursement"] = d;
            return View("DisbursementDetail");
        }

        //approve disbursement by employee
        [HttpGet, Route("approvedisbursements/{disbursementId}")]
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
        [AuthorizeFilter((int)UserRank.Clerk)]
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
        [AuthorizeFilter((int)UserRank.Clerk)]
        public ActionResult Deliveries()
        {
            int userId = Convert.ToInt32(RouteData.Values["userId"]);
            List<Disbursement> disbursements = DisbursementDao.GetPreparedDisbursements(userId);
            ViewData["Disbursements"] = disbursements;
            return View("Deliveries");
        }


        //retrieving and preparing for delivery
        [HttpPost]
        [AuthorizeFilter((int)UserRank.Clerk)]
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
        [AuthorizeFilter((int)UserRank.Manager, (int)UserRank.Supervisor, (int)UserRank.Clerk)]
        public ActionResult GenerateInvoices()
        {
            List<Department> departments = DepartmentDao.GetAllDepartments();

            Dictionary<int, string> monthDict = new Dictionary<int, string>();
            foreach( var i in Enum.GetValues(typeof(Months)))
            {
                monthDict.Add((int)i, i.ToString());
            }
            ViewData["Departments"] = departments;
            ViewData["MonthDict"] = monthDict;
            return View("Invoices");
        }

        [HttpGet,Route("disbursements")]
        public ActionResult GetDisbursementListWithMonth(int requestedDepartmentId, int month)
        {
            Debug.WriteLine("Department Id is " + requestedDepartmentId + " month is " + month);
            List<Disbursement> disbursements = DisbursementDao.GetDisbursementsByDepartmentAndMonth(requestedDepartmentId, month,(int)DisbursementStatus.Approved);
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

        [HttpGet, Route("generateinvoice")]
        public ActionResult GenerateInvoice(int departmentId, int month)
        {
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

        //employee can watch disbursements by department
        [HttpGet,Route("departmentdisbursements", Name = "departmentdisbursements")]
        public ActionResult GetAllDisbursementByDepartment()
        {
            int departmentId = Convert.ToInt32(RouteData.Values["departmentId"]);
            List<Disbursement> disbursements = DisbursementDao.GetAllDisbursementsByDepartment(departmentId);
            ViewData["Disbursements"] = disbursements;
            return View("DepartmentDisbursements");
        }

        //department disbursement detail
        [HttpGet, Route("departmentdisbursements/{disbursementId}")]
        public ActionResult GetDisbursementDetailByDepartment(int disbursementId)
        {
            int departmentId = Convert.ToInt32(RouteData.Values["departmentId"]);
            Disbursement d = DisbursementDao.GetDisbursementDetailById(departmentId);
            ViewData["Disbursement"] = d;
            return View("DepartmentDisbursementDetail");
        }

        [HttpGet,Route("prepareddisbursements", Name = "prepareddisbursements")]
        [AuthorizeFilter((int)UserRank.Employee, (int)UserRank.Head)]
        public ActionResult GetPreparedDisbursementsByDepartment()
        {
            int departmentId = Convert.ToInt32(RouteData.Values["departmentId"]);
            int userId = Convert.ToInt32(RouteData.Values["userId"]);
            List<Disbursement> disbursements = DisbursementDao.GetPreparedDisbursements(departmentId, userId);
            ViewData["Disbursements"] = disbursements;
            return View("PickUpDisbursements");

        }


        //approved disbursements for invoice

        [HttpGet, Route("approveddisbursements/{disbursementId}")]
        [AuthorizeFilter((int)UserRank.Manager, (int)UserRank.Supervisor, (int)UserRank.Clerk)]
        public ActionResult ApprovedDisbursements(int disbursementId)
        {
            Disbursement d = DisbursementDao.GetDisbursementDetailById(disbursementId);

            ViewData["Disbursement"] = d;

            return View("ApprovedDisbursementDetail");
        }



        public ActionResult DeliveriesMobile()
        {
            int userId = Convert.ToInt32(RouteData.Values["userId"]);
            List<Disbursement> retrievedList = DisbursementDao.GetPreparedDisbursementsForMobile(userId);
            List<object> disbursementList = new List<object>();
            foreach (Disbursement d in retrievedList)
            {
                List<object> dDetailsList = new List<object>();
                foreach (var disbursementDetail in d.DisbursementDetails)
                {
                    dDetailsList.Add(new
                    {
                        DisbursementDetailId = disbursementDetail.DisbursementDetailId,
                        ItemName = disbursementDetail.Item.Description,
                        ItemId = disbursementDetail.Item.ItemId,
                        Quantity = disbursementDetail.Quantity
                    });
                }
                var temp = new
                {
                    DisbursementId = d.DisbursementId,
                    Representative = d.Department.Representative.Name,
                    DepartmentName = d.Department.DepartmentName,
                    DisbursementDetails = dDetailsList
                };
                disbursementList.Add(temp);
            }
            return Json(disbursementList, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DisbursementsMobile(List<RetrievalItem> list)
        {
            ActionResult ar = Disbursements(list);
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult ReceivingMobile(List<DisbursementDetail> details)
        {
            int ddid = details[0].DisbursementDetailId;
            int id = DisbursementDao.GetDisbursementByDetailId(ddid).DisbursementId;

            Disbursement d = DisbursementDao.GetDisbursement(id);
            foreach (var item in d.DisbursementDetails)
            {
                foreach (var detail in details)
                {
                    Console.WriteLine("Before: " + item.DisbursementDetailId + "----" + item.Quantity);
                    if (detail.DisbursementDetailId == item.DisbursementDetailId)
                    {

                        item.Quantity = detail.Quantity;

                    }
                    Console.WriteLine("After: " + item.DisbursementDetailId + "----" + item.Quantity);
                }
            }

            ReceiveItemsByDepartment(d.DisbursementDetails, d.DisbursementId);

            return Json(details, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Route("delivereddisbursementsmobile")]
        public ActionResult GetDeliveredDisbursementsMobile()
        {
            int departmentId = Convert.ToInt32(RouteData.Values["departmentId"]);
            int userId = Convert.ToInt32(RouteData.Values["userId"]);
            List<Disbursement> disbursements = DisbursementDao.GetDisbursementsByDepartmentAndMonth(userId, departmentId, DateTime.Now.Month, (int)DisbursementStatus.Delivered);
            List<object> disbursementList = new List<object>();
            foreach (Disbursement d in disbursements)
            {
                List<object> dDetailsList = new List<object>();
                foreach (var disbursementDetail in d.DisbursementDetails)
                {
                    dDetailsList.Add(new
                    {
                        DisbursementDetailId = disbursementDetail.DisbursementDetailId,
                        ItemName = disbursementDetail.Item.Description,
                        ItemId = disbursementDetail.Item.ItemId,
                        Quantity = disbursementDetail.Quantity
                    });
                }
                var temp = new
                {
                    DepartmentName = d.Department.DepartmentName,
                    DisbursementId = d.DisbursementId,
                    DisbursementDetails = dDetailsList
                };
                disbursementList.Add(temp);
            }
            return Json(disbursementList, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Route("delivereddisbursementsmobile/{disbursementId}")]
        public ActionResult GetDeliveredDisbursementDetailByIdMobile(int disbursementId)
        {
            int departmentId = Convert.ToInt32(RouteData.Values["departmentId"]);
            Disbursement d = DisbursementDao.GetDeliveredDisbursementById(disbursementId);
            return Json(d, JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Route("approvedisbursementsmobile/{disbursementId}")]
        public ActionResult ApproveDisbursementByIdMobile(int disbursementId)
        {
            ApproveDisbursementById(disbursementId);
            List<object> response = new List<object>();
            response.Add("Success");
            return Json(response, JsonRequestBehavior.AllowGet);

        }

    }
}