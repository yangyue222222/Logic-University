using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class OrderDetail
    {
        public int OrderDetailId { get; set; }
        public Item Item { get; set; }
        public int OrderQuantity { get; set; }
        public int DeliveredQuantity { get; set; }
        public Order Order { get; set; }
    }
}