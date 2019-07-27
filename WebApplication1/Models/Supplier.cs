using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public enum Priority
    {
        Default,
        Second,
        Third
    }

    public class Supplier
    {
        public int SupplierId { get; set; }
        public string Name { get; set; }
        public string PhNo { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public int Priority { get; set; }
    }
}