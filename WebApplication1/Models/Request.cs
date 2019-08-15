using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public enum RequestStatus
    {
        Requested,
        Approved,
        Rejected,
        Cancelled,
        FullyDelivered,
        PartiallyDelivered,
    }

    public enum RequestRetrievalStatus {
        Prepared,
        NotPrepared
    }


    public class Request
    {
        public int RequestId { get; set; }
        public DateTime Date { get; set; }
        public User Requestor { get; set; }
        public int Status { get; set; }
        public User ApprovedBy { get; set; }
        public Department Department { get; set; }
        public int DisbursementStatus { get;set; }
        public ICollection<RequestDetail> RequestDetails { get; set; }
        public ICollection<Disbursement> Disbursements { get; set; }
    }
}