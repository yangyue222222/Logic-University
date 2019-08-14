using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.DAOs;
using WebApplication1.Models;
using System.Diagnostics;
using WebApplication1.Filters;

namespace WebApplication1.Controllers
{
    [AuthFilter]
    public class StockController : Controller
    {
        [HttpGet,Route("receivestocks")]
        public ActionResult Index()
        {
            int userId = Convert.ToInt32(RouteData.Values["userId"]);
            List<Order> orders = OrderDao.GetApprovedOrders(userId);
            ViewData["ApprovedOrders"] = orders;
            return View("StockOrders");
        }

        [HttpGet,Route("receivestocks/{stockId}")]
        public ActionResult StockOrderById(int stockId)
        {

            Order order = OrderDao.GetOrderById(stockId);
            ViewData["Order"] = order;
            return View("StockOrderDetail");
        }

        [HttpPost, Route("receivestocks/{stockId}")]
        public ActionResult ReceiveStock(List<OrderDetail> details,int stockId)
        {

            Order order = new Order()
            {
                OrderId = stockId,
                OrderDetails = details
            };

            foreach(var od in details)
            {
                Debug.WriteLine(od.OrderDetailId + "--" + od.DeliveredQuantity);
            }

            return RedirectToAction("Index");
        }

        [HttpGet, Route("instocks",Name = "instocks")]
        [AuthorizeFilter((int)UserRank.Manager,(int)UserRank.Supervisor,(int)UserRank.Clerk)]
        public ActionResult Inventory()
        {
            ViewData["Items"] = ItemDao.GetAllItems();
            return View("Inventory");
        }

    }
}