﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public enum Months
    {
        January = 1,
        February,
        March,
        April,
        May,
        June,
        July,
        August,
        September,
        October,
        November,
        December
    }

    public enum DisbursementStatus
    {
        Prepared,
        Delivered,
        Approved,
        InvoiceGenerated
    }

    public class Disbursement
    {
        public int DisbursementId { get; set; }
        public Department Department { get; set; }
        public DateTime Date { get; set; }
        public Request Request { get; set; }
        public User ApprovedBy { get; set; }
        public int Status { get; set; }
        public List<DisbursementDetail> DisbursementDetails { get; set; }
    }
}