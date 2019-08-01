using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.Models;
using WebApplication1.DataAccessLayer;
using System.Diagnostics;

namespace WebApplication1.DAOs
{
    public class RequestDao
    {
        public static void InsertRequest(ICollection<Item> items, User u)
        {
            try
            {
                using (var ctx = new UniDBContext())
                {
                    Request req = new Request();
                    req.Requestor = u;
                    req.Department = u.Department;
                    ICollection<RequestDetail> details = new List<RequestDetail>();
                    req.Date = DateTime.Now;
                    req.Status = (int)RequestStatus.Requested;
                    foreach (var i in items)
                    {
                        RequestDetail detail = new RequestDetail();
                        ctx.Items.Attach(i);
                        detail.Item = i;
                        detail.Quantity = i.Quantity;
                        details.Add(detail);
                    }

                    ctx.Users.Attach(u);
                    ctx.Departments.Attach(req.Department);
                    req.RequestDetails = details;
                    ctx.RequestDetails.AddRange(details);
                    ctx.Requests.Add(req);
                    ctx.SaveChanges();
                }
            }catch (Exception e)
            {
                throw new Exception("Internal Server Error");
            }
        }

        public static List<Request> getRequestsByDepartment(int departmentId)
        {
            try
            {
                using(var ctx = new UniDBContext())
                {
                    List<Request> requests = ctx.Requests.Include("Requestor").Where(r => r.Requestor.Department.DepartmentId == departmentId && r.Status == (int)RequestStatus.Requested).ToList();
                    
                    return requests;
                }
            }catch(Exception e)
            {
                return null;
            }
        }

        public static List<RequestDetail> getRequestDetailsByRequestId(int id,int departmentId)
        {
            List<RequestDetail> details = null;
            using(var ctx = new UniDBContext())
            {
                details = ctx.RequestDetails.Include("Item").Where(d => d.Request.RequestId == id && d.Request.Department.DepartmentId == departmentId).ToList();
            }
            return details;
        }

        public static void ApproveRequest(Request request)
        {
            
            using (var ctx = new UniDBContext())
            {
                var req = ctx.Requests.Include("RequestDetails").Include("RequestDetails.Item").Where(r => r.RequestId == request.RequestId && r.Department.DepartmentId == request.Department.DepartmentId).SingleOrDefault();
                req.Status = request.Status;
                req.ApprovedBy = request.ApprovedBy;
                ctx.Users.Attach(req.ApprovedBy);
                ctx.Departments.Attach(request.Department);

                if(req.Status == (int)RequestStatus.Approved)
                {
                    //get list of item ids
                    Disbursement disbursement = new Disbursement();
                    List<int> itemIds = req.RequestDetails.Select(rd => rd.Item.ItemId).ToList();
                    List<DisbursementDetail> disbursementDetails = new List<DisbursementDetail>();

                    //get the current stock items 
                    IDictionary<int, Item> itemsDictionary = ctx.Items.Where(i => itemIds.Contains(i.ItemId)).ToDictionary(i => i.ItemId, i => i);

                    foreach(var rD in req.RequestDetails)
                    {
                        Item i = itemsDictionary[rD.Item.ItemId];
                       
                        if(i.Quantity != 0) {

                            if (rD.Quantity > i.Quantity)
                            {
                                //requested amount is more than current stock
                                disbursement.Status = (int)DisbursementStatus.Allocated;
                                req.Status = (int)RequestStatus.PartiallyAllocated;
                                DisbursementDetail dd = new DisbursementDetail()
                                {
                                    Item = i,
                                    Quantity = i.Quantity
                                };

                                i.Quantity = 0;
                                disbursementDetails.Add(dd);
                            }
                            else
                            {
                                DisbursementDetail dd = new DisbursementDetail()
                                {
                                    Item = i,
                                    Quantity = rD.Quantity,
                                };

                                i.Quantity = i.Quantity - rD.Quantity;
                                disbursementDetails.Add(dd);
                            }
                        }
                        
                    }

                    if (disbursementDetails.Count > 0)
                    {
                        disbursement.DisbursementDetails = disbursementDetails;
                        disbursement.Date = DateTime.Now.AddDays(-1);
                        disbursement.Department = request.Department;
                        disbursement.Request = req;
                        ctx.Disbursements.Add(disbursement);
                        ctx.DisbursementDetails.AddRange(disbursementDetails);

                        if (req.Status != (int)RequestStatus.PartiallyAllocated)
                        {
                            req.Status = (int)RequestStatus.FullyAllocated;
                        }
                    }
                    
                }
                try
                {
                    ctx.SaveChanges();

                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.InnerException.Message);
                    Debug.WriteLine(e.InnerException.StackTrace.ToString());
                }
            }
        }

        public static List<RetrievalItem> GetRequestDeailsByItems()
        {
            using(var ctx = new UniDBContext())
            {
                List<RetrievalItem> items = ctx.RequestDetails.Include("Request").Include("Item")
                    .Where(r =>
                        r.Request.Date.CompareTo(DateTime.Now) < 0 && r.Request.Status == (int)RequestStatus.Approved
                    ).GroupBy(r =>new { r.Item.Description, r.Item.ItemId })
                    .Select(r => new RetrievalItem
                    {
                        Description = r.Key.Description,
                        AllocatedQuantity = r.Sum(rq => rq.Quantity),
                        ItemId = r.Key.ItemId
                    }).ToList();

                return items;
            }
        }

        public static List<Request> GetRequestDetailByDepartment()
        {
            using (var ctx = new UniDBContext())
            {
                List<Request> requests = ctx.Requests.Include("RequestDetails").Include("RequestDetails.Item").Include("Department").Where(r => r.Status == (int)RequestStatus.Approved).ToList();
                return requests;
            }
        }

        public static List<Request> GetMyRequests(User u)
        {
            using (var ctx = new UniDBContext())
            {
                List<Request> requests = ctx.Requests.Include("RequestDetails").Include("Department").Include("Requestor")
                    .Where(r => r.Department.DepartmentId == u.Department.DepartmentId && r.Requestor.UserId == u.UserId)
                    .OrderByDescending(r => r.RequestId).ToList();

                return requests;
            }
        }

       public static void CancelRequestById(int requestId,User u)
        {
            using(var ctx = new UniDBContext())
            {
                Request request = ctx.Requests.Where(r => r.RequestId == requestId && r.Department.DepartmentId == u.Department.DepartmentId 
                && r.Requestor.UserId == u.UserId && r.Status == (int)RequestStatus.Requested)
                    .SingleOrDefault();
                if (request != null)
                {
                    request.Status = (int)RequestStatus.Cancelled;
                    ctx.SaveChanges();
                }
            }
        }

        public static Request GetRequestById(int requestId,User u)
        {
            using(var ctx = new UniDBContext())
            {
                IQueryable<Request> query = ctx.Requests.Include("Requestor").Include("RequestDetails").Include("ApprovedBy").Include("Department").Include("RequestDetails.Item");
                if(u.Rank == (int)UserRank.Clerk)
                {
                    query = query.Where(r => r.RequestId == requestId);
                }
                else
                {
                    query = query.Where(r => r.RequestId == requestId && r.Requestor.UserId == u.UserId && r.Department.DepartmentId == u.Department.DepartmentId);
                }

                return query.SingleOrDefault();
            }
        }

    }
}