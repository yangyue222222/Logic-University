using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using WebApplication1.DataAccessLayer;
using WebApplication1.Models;

namespace WebApplication1.DAOs
{
    public class OrderDao
    {
        public static void InsertOrders(List<Item> items, User u,int supplierId)
        {
            try
            {
                using (var ctx = new UniDBContext())
                {
                    Supplier s = new Supplier()
                    {
                        SupplierId = supplierId
                    };

                    List<OrderDetail> orderDetails = new List<OrderDetail>();
                    foreach (var i in items)
                    {
                        ctx.Items.Attach(i);
                        OrderDetail oD = new OrderDetail()
                        {
                            Item = i,
                            OrderQuantity = i.Quantity
                        };
                        orderDetails.Add(oD);
                    }

                    ctx.Users.Attach(u);
                    ctx.Suppliers.Attach(s);

                    Order o = new Order()
                    {
                        OrderDetails = orderDetails,
                        Date = DateTime.Now,
                        Requestor = u,
                        Status = (int)OrderStatus.Requested,
                        Supplier = s
                    };

                    ctx.Orders.Add(o);
                    ctx.SaveChanges();
                }
            }
            catch(Exception e)
            {
                throw new Exception("Internal Server Error");
            }
            
        }


        public static List<PendingOrder> GetPendingOrders()
        {
            using (var ctx = new UniDBContext())
            {
                List<PendingOrder> pendingOrders = ctx.Orders.Include("Requestor")
                    .Include("OrderDetails")
                    .Include("OrderDetails.Item")
                    .Where(o => o.Status == (int)OrderStatus.Requested).OrderBy(o => o.Date)
                    .Select(o => new PendingOrder
                    {
                        OrderId = o.OrderId,
                        SubmittedTime = o.Date,
                        RaisedBy = o.Requestor.Name,
                        Total = o.OrderDetails.Sum(od => (od.OrderQuantity * od.Item.Price))
                    }).ToList();

                return pendingOrders;
            }
        }

        public static Order GetOrderById(int orderId)
        {
            using (var ctx = new UniDBContext())
            {
                Order order = ctx.Orders.Include("OrderDetails")
                    .Include("OrderDetails.Item")
                    .Where(o => o.OrderId == orderId).SingleOrDefault();

                return order;
            }
        }

        public static void UpdateOrderStatus(Order order)
        {
            using (var ctx = new UniDBContext())
            {
                Order or = ctx.Orders.Where(o => o.OrderId == order.OrderId).SingleOrDefault();
                or.ApprovedBy = order.ApprovedBy;
                or.Status = order.Status;
                ctx.Users.Attach(or.ApprovedBy);
                ctx.SaveChanges();
            }
        }

    }
}