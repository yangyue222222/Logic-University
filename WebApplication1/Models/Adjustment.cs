using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public enum AdjustmentStatus
    {
        Raised,
        Approved
    }


    public class Adjustment
    {
        public int AdjustmentId { get; set; }
        public User Requestor { get; set; }
        public DateTime RaisedTime { get; set; }
        public int Status { get; set; }
        public int Total { get; set; }
        public User ApprovedBy { get; set; }
        public List<AdjustmentDetail> AdjustmentDetails { get; set; }
    }
}