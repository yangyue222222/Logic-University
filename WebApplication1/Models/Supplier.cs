using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{

    public class Supplier
    {
        public int SupplierId { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public string ContactName { get; set; }
        public string PhoneNo { get; set; }
        public string FaxNo { get; set; }
        public string Address { get; set; }
        public int Priority { get; set; }
    }
}