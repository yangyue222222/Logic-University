using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.DAOs;
using WebApplication1.Models;
using System.Diagnostics;
using WebApplication1.Utilities;

namespace WebApplication1.Controllers
{
    public class OrdersController : Controller
    {
        // GET: Order
        [HttpGet]
        public ActionResult Index()
        {
            ItemDao itemDao = new ItemDao();
            Dictionary<string, List<Item>> items = itemDao.getItemsForRequisition();
            List<Supplier> suppliers = SupplierDao.GetSuppliers();

            ViewData["Items"] = items;
            ViewData["Suppliers"] = suppliers;
            return View();
        }

        [HttpPost]
        public ActionResult Index(List<Item> items,int supplierId)
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

        [HttpGet]
        public ActionResult PendingOrders()
        {
            List<PendingOrder> pendingOrders = OrderDao.GetPendingOrders();
            if (pendingOrders != null)
            {
                ViewData["PendingOrders"] = pendingOrders;

            }

            return View();
        }

        [HttpGet]
        public ActionResult OrderDetail(int orderId)
        {
            Order order = OrderDao.GetOrderById(orderId);

            ViewData["Order"] = order;
            return View();
        }

        [HttpPost,Route("orders/pendingorders/{orderId}")]
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