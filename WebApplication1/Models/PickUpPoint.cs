using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class PickUpPoint
    {
        public int PickUpPointId { get; set; }
        public string PickUpTime { get; set; }
        public string Location { get; set; }
        public User StoreClerk { get; set; }
    }
}