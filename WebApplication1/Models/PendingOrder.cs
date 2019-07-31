using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    [NotMapped]
    public class PendingOrder
    {
        public int OrderId { get; set; }
        public string RaisedBy { get; set; }
        public int Total { get; set; }
        public DateTime SubmittedTime { get; set; }
    }
}