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
        [HttpGet,Route("orderitems")]
        public ActionResult Index()
        {
            Dictionary<string, List<Item>> items = ItemDao.getItemsForRequisition();
            List<Supplier> suppliers = SupplierDao.GetSuppliers();

            ViewData["Items"] = items;
            ViewData["Suppliers"] = suppliers;
           
            return View("OrderItems");
        }

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
                    string token = HttpContext.Request.Cookies["token"].Value;
                    string decryptedToken = TokenUtility.Decrypt(token);
                    string[] arr = decryptedToken.Split(new string[] { "%" }, StringSplitOptions.None);
                    User user = new User()
                    {
                        UserId = Convert.ToInt32(arr[0])
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

        [HttpGet,Route("orders")]
        public ActionResult GetAllOrders()
        {
            List<Order> orders = OrderDao.GetAllOrders();

            ViewData["Orders"] = orders;
            return View("Orders");
        }

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
        

        [HttpGet,Route("approveorders")]
        public ActionResult ApprovedOrders()
        {
            List<Order> orders = OrderDao.GetApprovedOrders();
            ViewData["Orders"] = orders;
            return View();
        }

        [HttpPost,Route("approveorders/{orderId}")]
        public ActionResult ApprovedOrders(List<OrderDetail> orderDetails,int orderId)
        {
            OrderDao.ReceiveStocks(orderDetails,orderId);
            return RedirectToAction("ApprovedOrders");
        }

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

        [HttpPost,Route("pendingorders/{orderId}")]
        public ActionResult UpdateOrder(int orderId,string status)
        {

            string token = HttpContext.Request.Cookies["token"].Value;
            string decryptedToken = TokenUtility.Decrypt(token);
            string[] arr = decryptedToken.Split(new string[] { "%" }, StringSplitOptions.None);

            User u = new User()
            {
                UserId = Convert.ToInt32(arr[0])
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