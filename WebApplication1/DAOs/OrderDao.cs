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
                    .Include("Supplier").Include("Requestor")
                    .Include("OrderDetails.Item").Include("ApprovedBy")
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

        public static List<Order> GetApprovedOrders()
        {
            using (var ctx = new UniDBContext())
            {
                List<Order> approvedOrders = ctx.Orders.Include("Supplier").Include("OrderDetails").Include("OrderDetails.Item").Include("Requestor")
                    .Where(o => o.Status == (int)OrderStatus.Approved)
                    .ToList();
                return approvedOrders;
            }
        }

        public static void ReceiveStocks(List<OrderDetail> orderDetails,int orderId)
        {
            using (var ctx = new UniDBContext())
            {
                List<int> orderDetailIds = orderDetails.Select(od => od.OrderDetailId).ToList();

                Order order= ctx.Orders.Include("Requestor").Include("OrderDetails").Include("OrderDetails.Item")
                    .Where(o => o.OrderId == orderId)
                    .SingleOrDefault();
                if(order != null)
                {
                    Dictionary<int, OrderDetail> orderDetailDict = order.OrderDetails.ToDictionary(od => od.OrderDetailId);
                    order.Status = (int)OrderStatus.Delivered;
                    foreach (var od in orderDetails)
                    {
                        OrderDetail orderDetail = orderDetailDict[od.OrderDetailId];
                        if(od.DeliveredQuantity == 0)
                        {
                            ctx.OrderDetails.Remove(orderDetail);
                        }else
                        {
                            orderDetail.DeliveredQuantity = od.DeliveredQuantity;
                            Item i = orderDetail.Item;
                            i.Quantity += od.DeliveredQuantity;
                        }
                        
                    }

                    ctx.SaveChanges();
                }
                


                //List<int> orderDetailsIds = orderDetails.Select(od => od.OrderDetailId).ToList();
                //List<int> itemIds = ctx.OrderDetails.Include("Item").Where(od => orderDetailsIds.Contains(od.OrderDetailId)).Select(od => od.Item.ItemId).ToList();

                //Order order = ctx.Orders.Where(o => o.OrderId == orderId).SingleOrDefault();
                //order.Status = (int)OrderStatus.Delivered;

                //Dictionary<int, OrderDetail> details = ctx.OrderDetails.Where(od => orderDetailsIds.Contains(od.OrderDetailId)).ToDictionary(x => x.OrderDetailId);
               
                ////allocate requests which got the delivered item and the status should be in partially delivered,partially allocated, approved
                //Dictionary<int, bool> requestStatusDict = new Dictionary<int, bool> {
                //    { (int)RequestStatus.Approved,true} ,
                //    { (int)RequestStatus.PartiallyAllocated,true},
                //    { (int)RequestStatus.PartiallyDelivered,true },
                //};

                //List<Disbursement> disbursement = new List<Disbursement>();

                ////get dictionary with key of item and requestdetail
                //Dictionary<int, List<RequestDetail>> requests = ctx.RequestDetails.Include("Request").Include("Item")
                //        .Where(rd => requestStatusDict.ContainsKey(rd.Request.Status) && itemIds.Contains(rd.Item.ItemId)).GroupBy(rd => rd.Item.ItemId).ToDictionary(x => x.Key,x => x.ToList());

                ////update orderdetail delivered quantity
                //foreach (var od in orderDetails)
                //{
                //    OrderDetail orderDetail = details[od.OrderDetailId];
                //    orderDetail.DeliveredQuantity = od.DeliveredQuantity;
                //    orderDetail.Order = order;
                //}
                //ctx.SaveChanges();

                //Order updatedOrder = ctx.Orders.Include("OrderDetails").Include("OrderDetails.Item").Where(o => o.OrderId == orderId).SingleOrDefault();

                ////collect the allocated request
                //HashSet<int> requestIds = new HashSet<int>();

                ////request id as key and disbursement detail as value for generating disbursement
                //Dictionary<int, List<DisbursementDetail>> disbursementDict = new Dictionary<int, List<DisbursementDetail>>();


                //foreach(var od in updatedOrder.OrderDetails)
                //{
                //    int qty = od.DeliveredQuantity;
                //    int itemId = od.Item.ItemId;
                //    List<RequestDetail> rDetails = requests[itemId];
                //    //iterate through request detail 
                //    foreach (var rD in rDetails)
                //    {
                //        if(qty != 0)
                //        {
                //            if(rD.Request.Status == (int)RequestStatus.PartiallyDelivered)
                //            {
                //                //disbursement details for item
                //                Request r = ctx.Requests.Include("Disbursements").Include("Disbursements.DisbursementDetails").Include("Disbursements.DisbursementDetails.Item")
                //                    .Where(rdd => rdd.RequestId == rD.Request.RequestId).SingleOrDefault();

                //                List<DisbursementDetail> sameItemDetail = new List<DisbursementDetail>();
                //                foreach(var eachDisbursement in r.Disbursements)
                //                {
                //                    foreach(var ddetail in eachDisbursement.DisbursementDetails)
                //                    {
                //                        if(ddetail.Item.ItemId == itemId)
                //                        {
                //                            sameItemDetail.Add(ddetail);
                //                            break;
                //                        }
                //                    }
                //                }

                //                int deliveredAmount = 0;
                //                foreach(var ddetail in sameItemDetail)
                //                {
                //                    deliveredAmount += ddetail.Quantity;
                //                }

                //                var requiredAmount = 

                                

                //            }
                //        }else
                //        {
                //            break;
                //        }
                //    }
                    
                //}



            }
        }

        public static List<Order> GetAllOrdersByUserId(int userId)
        {
            using(var ctx = new UniDBContext())
            {
                List<Order> orders = ctx.Orders.Include("Requestor").Include("Supplier")
                    .Where(o => o.Requestor.UserId == userId)
                    .OrderByDescending(o => o.OrderId).ToList();
                return orders;
            }
        }

        public static void CancelOrderById(int userId, int orderId)
        {
            using (var ctx = new UniDBContext())
            {
                Order order = ctx.Orders.Where(o => o.OrderId == orderId && o.Status == (int)OrderStatus.Requested).SingleOrDefault();
                if(order != null)
                {
                    order.Status = (int)OrderStatus.Cancelled;
                    ctx.SaveChanges();
                }
            }
        }

        public static List<object> GetOrderedItemsByMonth(int month, int supplierId)
        {
            try
            {
                using (var ctx = new UniDBContext())
                {
                    List<Order> orders = ctx.Orders.Include("Supplier").Include("OrderDetails").Include("OrderDetails.Item")
                        .Where(o => o.Date.Year == DateTime.Now.Year && o.Date.Month == month && o.Status == (int)OrderStatus.Delivered)
                        .ToList();
                    if (supplierId != 0)
                    {
                        orders = orders.Where(o => o.Supplier.SupplierId == supplierId).ToList();
                    }
                    Dictionary<int, int> itemInfo = new Dictionary<int, int>();

                    foreach (var o in orders)
                    {
                        foreach (var oD in o.OrderDetails)
                        {
                            var requiredAmount = oD.DeliveredQuantity;
                            if (itemInfo.ContainsKey(oD.Item.ItemId))
                            {
                                requiredAmount += itemInfo[oD.Item.ItemId];
                                itemInfo[oD.Item.ItemId] = requiredAmount;
                            }
                            else
                            {
                                itemInfo.Add(oD.Item.ItemId, requiredAmount);
                            }
                        }
                    }

                    List<int> itemIds = itemInfo.Keys.ToList();
                    Dictionary<int, Item> stockDict = ctx.Items.Where(i => itemIds.Contains(i.ItemId)).ToDictionary(i => i.ItemId);
                    List<object> orderlist = new List<object>();
                    foreach (var itemId in itemIds)
                    {
                        int requestedNumber = itemInfo[itemId];
                        Item i = stockDict[itemId];

                        object orderitem = new 
                        {
                            Description = i.Description,
                            Qty = requestedNumber,
                            ItemId = i.ItemId,
                            Price = i.Price,
                        };

                        orderlist.Add(orderitem);

                    }
                    return orderlist;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

    }
}