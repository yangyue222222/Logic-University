using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class AdjustmentDetail
    {
        public int AdjustmentDetailId { get; set; }
        public Item Item { get; set; }
        public int Count { get; set; }
        public Adjustment Adjustment { get; set; }
    }
}