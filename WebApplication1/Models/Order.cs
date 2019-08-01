using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public enum OrderStatus
    {
        Requested,
        Cancelled,
        Approved,
        Rejected,
        Delivered
    }
    public class Order
    {
        public int OrderId { get; set; }
        public User Requestor { get; set; }
        public DateTime Date { get; set; }
        public int Status { get; set; }
        public User ApprovedBy { get; set; }
        public Supplier Supplier { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
    }
}