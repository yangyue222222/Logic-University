using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.DAOs;
using WebApplication1.Models;
using System.Diagnostics;
using WebApplication1.Utilities;
using WebApplication1.Filters;

namespace WebApplication1.Controllers
{
    [AuthFilter]
    public class OrdersController : Controller
    {
        // GET: Order

        //store man order item page
        [HttpGet,Route("orderitems",Name = "orderitems")]
        [AuthorizeFilter((int)UserRank.Clerk,(int)UserRank.Manager,(int)UserRank.Supervisor)]
        public ActionResult Index()
        {
            Dictionary<string, List<Item>> items = ItemDao.getItemsForRequisition();
            List<Supplier> suppliers = SupplierDao.GetSuppliers();

            ViewData["Items"] = items;
            ViewData["Suppliers"] = suppliers;
           
            return View("OrderItems");
        }

        //storeman order item page
        [HttpPost,Route("orderitems")]
        [AuthorizeFilter((int)UserRank.Clerk,(int)UserRank.Manager, (int)UserRank.Supervisor)]
        public ActionResult Index(List<Item> items, int supplierId)
        {

            if (items != null)
            {
                foreach (var i in items)
                {
                    if (i.ItemId == 0 || i.Quantity < 1)
                    {
                        return new HttpStatusCodeResult(400);
                    }

                }

                try
                {
                    int userId = Convert.ToInt32(RouteData.Values["userId"]);
                    User user = new User()
                    {
                        UserId = userId
                    };

                    OrderDao.InsertOrders(items, user, supplierId);
                    return new HttpStatusCodeResult(200);
                }
                catch (Exception e)
                {
                    return new HttpStatusCodeResult(400);
                }

            }
            return new HttpStatusCodeResult(400);
        }

        //get all orders
        [HttpGet,Route("orders")]
        [AuthorizeFilter((int)UserRank.Manager, (int)UserRank.Supervisor, (int)UserRank.Clerk)]
        public ActionResult GetAllOrders()
        {
            int userId = Convert.ToInt32(RouteData.Values["userId"]);
            List<Order> orders = OrderDao.GetAllOrdersByUserId(userId);

            ViewData["Orders"] = orders;
            return View("Orders");
        }
        

        //get a particular order
        [HttpGet,Route("orders/{orderId}")]
        [AuthorizeFilter((int)UserRank.Manager, (int)UserRank.Supervisor, (int)UserRank.Clerk)]
        public ActionResult OrderDetail(int orderId)
        {
            Order order = OrderDao.GetOrderById(orderId);
            if (order != null)
            {
                ViewData["Order"] = order;
                return View("OrderDetail");
            }
            return RedirectToAction("GetAllOrders");
        }

        [HttpPost,Route("cancelorders/{orderId}")]
        [AuthorizeFilter((int)UserRank.Manager, (int)UserRank.Supervisor, (int)UserRank.Clerk)]
        public ActionResult CancelOrder(int orderid)
        {
            int userId = Convert.ToInt32(RouteData.Values["userId"]);
            OrderDao.CancelOrderById(userId, orderid);
            return RedirectToAction("GetAllOrders");
        }
        

        //get all approved orders for receiving stock
        [HttpGet,Route("approvedorders",Name = "approvedorders")]
        [AuthorizeFilter((int)UserRank.Clerk)]
        public ActionResult ApprovedOrders()
        {
            List<Order> orders = OrderDao.GetApprovedOrders();
            ViewData["Orders"] = orders;
            return View();
        }

        //post order for receiving stock
        [HttpPost, Route("approveorders/{orderId}")]
        [AuthorizeFilter((int)UserRank.Clerk)]
        public ActionResult ApprovedOrders(List<OrderDetail> orderDetails, int orderId)
        {
            bool validOrder = true;
            foreach (var od in orderDetails)
            {
                if(od.DeliveredQuantity < 0)
                {
                    validOrder = false;
                    break;
                }
            }

            if (validOrder)
            {
                OrderDao.ReceiveStocks(orderDetails, orderId);
            }
            return RedirectToAction("ApprovedOrders");
        }


        //for storemanager or store supervisor
        [HttpGet,Route("PendingOrders",Name = "PendingOrders" )]
        [AuthorizeFilter((int)UserRank.Manager,(int)UserRank.Supervisor)]
        public ActionResult PendingOrders()
        {
            List<PendingOrder> pendingOrders = OrderDao.GetPendingOrders();
            if (pendingOrders != null)
            {
                ViewData["PendingOrders"] = pendingOrders;

            }
            return View();
        }

        //for storemanager or store supervisor
        [HttpGet,Route("PendingOrders/{orderId}")]
        [AuthorizeFilter((int)UserRank.Manager, (int)UserRank.Supervisor)]
        public ActionResult PendingOrder(int orderId)
        {
            Order order = OrderDao.GetOrderById(orderId);
            if(order != null)
            {
                ViewData["Order"] = order;
                return View("PendingOrderDetail");
            }
            return RedirectToAction("PendingOrders");
        }

        //for storemanager or store supervisor to approve orders
        [HttpPost,Route("pendingorders/{orderId}")]
        [AuthorizeFilter((int)UserRank.Manager, (int)UserRank.Supervisor)]
        public ActionResult UpdateOrder(int orderId,string status)
        {

            int userId = Convert.ToInt32(RouteData.Values["userId"]);

            User u = new User()
            {
                UserId = Convert.ToInt32(userId)
            };

            Order order = new Order()
            {
                OrderId = orderId,
                ApprovedBy = u
            };
            if (status.Equals("ACCEPT")){
                order.Status = (int)OrderStatus.Approved;
            }else
            {
                order.Status = (int)OrderStatus.Rejected;
            }

            OrderDao.UpdateOrderStatus(order);
            return RedirectToAction("PendingOrders");
        }

        [HttpGet, Route("priorityreorderitems")]
        [AuthorizeFilter((int)UserRank.Manager, (int)UserRank.Supervisor, (int)UserRank.Clerk)]
        public ActionResult GetPriorityItemsForReOrder()
        {
            List<RequestDetail> unfulfilledRequestDetails = RequestDao.GetUnfulfilledRequestDetails();

            //need to consider which requests are prepared but not delivered yet
            List<int> requestIdWithPreparedDisbursement = unfulfilledRequestDetails.Where(rd => rd.Request.DisbursementStatus == (int)RequestRetrievalStatus.Prepared)
                .Select(rd => rd.Request.RequestId).Distinct().ToList();

            //get prepared disbursement details
            List<DisbursementDetail> disbursementDetails = DisbursementDao.GetPreparedDisbursementByRequestIds(requestIdWithPreparedDisbursement);

            //get prepared item id as key and prepared amount as value in dictionary
            Dictionary<int,int> preparedItemsDict = disbursementDetails.GroupBy(dd => dd.Item.ItemId)
                .ToDictionary(cl => cl.Key, cl => cl.Sum(dd => dd.Quantity));

            //item id as key and required amount as dictionary value
            Dictionary<int, int> requiredItemDict = unfulfilledRequestDetails.GroupBy(rd => rd.Item.ItemId)
                .ToDictionary(cl => cl.Key,cl => cl.Sum(rd => (rd.Quantity - rd.DeliveredQuantity) ));
            //get item key to pass as an argument to finding item instock info
            List<int> itemIds = requiredItemDict.Keys.ToList();

            List<Item> currentStockItems = ItemDao.GetCurrentStockInfoByIds(itemIds);
            List<object> requiredItemsInfo = new List<object>();
            foreach(var i in currentStockItems)
            {
                int unfulfilledAmount = requiredItemDict[i.ItemId];
                if(i.Quantity == 0 || i.Quantity <= unfulfilledAmount)
                {
                    int preparedAmount = 0;
                    int requiredQty = 0;
                    int suggestedQty = 0;
                    if (preparedItemsDict.ContainsKey(i.ItemId))
                    {
                        preparedAmount = preparedItemsDict[i.ItemId];
                        requiredQty = unfulfilledAmount;
                        if(requiredQty == preparedAmount)
                        {
                            suggestedQty = 0;
                        }else
                        {
                            suggestedQty = requiredQty - preparedAmount + 10;
                        }
                    }
                    else
                    {
                        requiredQty = unfulfilledAmount - i.Quantity;
                        if(requiredQty == 0)
                        {
                            suggestedQty = 0;
                        }else
                        {
                            suggestedQty = requiredQty + 10;
                        }
                    }
                   
                    if(suggestedQty != 0)
                    {
                        var itemInfo = new
                        {
                            Description = i.Description,
                            ItemId = i.ItemId,
                            Category = i.Category,
                            SuggestedQty = suggestedQty,
                            UOM = i.UnitOfMeasure
                        };
                        requiredItemsInfo.Add(itemInfo);
                    }
                    
                }
            }

            return Json(requiredItemsInfo, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Route("orderreport", Name = "orderreport")]
        public ActionResult GenerateOrderReport()
        {
            List<Supplier> suppliers = SupplierDao.GetSuppliers();
            Dictionary<int, string> monthDict = new Dictionary<int, string>();
            foreach (var i in Enum.GetValues(typeof(Months)))
            {
                monthDict.Add((int)i, i.ToString());
            }
            ViewData["Suppliers"] = suppliers;
            ViewData["MonthDict"] = monthDict;
            return View("OrderReport");
        }
        [HttpGet, Route("orderhistory")]
        public ActionResult GetOrdersByMonth(int month, int supplierId)
        {

            List<object> monthlyorders = OrderDao.GetOrderedItemsByMonth(month, supplierId);
            return Json(new { results = monthlyorders }, JsonRequestBehavior.AllowGet);
        }
    }
}