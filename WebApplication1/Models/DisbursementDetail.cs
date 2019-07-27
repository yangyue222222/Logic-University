using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class DisbursementDetail
    {
        public int DisbursementDetailId { get; set; }
        public Disbursement Disbursement { get; set; }
        public Item Item { get; set; }
        public int Quantity { get; set; }
    }
}