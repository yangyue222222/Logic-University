using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class RequestDetail
    {
        public int RequestDetailId { get; set; }
        public Item Item { get; set; }
        public int Quantity { get; set; }
        public Request Request { get; set; }
        public int DeliveredQuantity { get; set; }
    }
}