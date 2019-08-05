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
        [HttpGet,Route("orderitems")]
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
        public ActionResult GetAllOrders()
        {
            List<Order> orders = OrderDao.GetAllOrders();

            ViewData["Orders"] = orders;
            return View("Orders");
        }
        

        //get a particular order
        [HttpGet,Route("orders/{orderId}")]
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
        public ActionResult CancelOrder(int orderid)
        {
            OrderDao.CancelOrderById(orderid);
            return RedirectToAction("GetAllOrders");
        }
        

        //get all approved orders for receiving stock
        [HttpGet,Route("approveorders")]
        public ActionResult ApprovedOrders()
        {
            List<Order> orders = OrderDao.GetApprovedOrders();
            ViewData["Orders"] = orders;
            return View();
        }

        //post order for receiving stock
        [HttpPost,Route("approveorders/{orderId}")]
        public ActionResult ApprovedOrders(List<OrderDetail> orderDetails,int orderId)
        {
            OrderDao.ReceiveStocks(orderDetails,orderId);
            return RedirectToAction("ApprovedOrders");
        }


        //for storemanager or store supervisor
        [HttpGet,Route("PendingOrders")]
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

        //for storemanager or store supervisor
        [HttpPost,Route("pendingorders/{orderId}")]
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
    }
}