using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.DAOs;
using WebApplication1.Models;
using System.Diagnostics;

namespace WebApplication1.Controllers
{
    public class StockController : Controller
    {
        [HttpGet,Route("receivestocks")]
        public ActionResult Index()
        {
            List<Order> orders = OrderDao.GetApprovedOrders();
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
        public ActionResult Inventory()
        {
            ViewData["Items"] = ItemDao.GetAllItems();
            return View("Inventory");
        }

    }
}