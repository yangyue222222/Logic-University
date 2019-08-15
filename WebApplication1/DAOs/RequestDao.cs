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
                    req.DisbursementStatus = (int)RequestRetrievalStatus.NotPrepared;
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
                try
                {
                    Request req = ctx.Requests.Where(r => r.RequestId == request.RequestId).SingleOrDefault();
                    req.Status = request.Status;
                    req.ApprovedBy = request.ApprovedBy;
                    req.Date = DateTime.Now.AddDays(-1);
                    ctx.Users.Attach(req.ApprovedBy);
                    ctx.SaveChanges();
                }                
                catch (Exception e)
                {
                    Debug.WriteLine(e.InnerException.Message);
                    Debug.WriteLine(e.InnerException.StackTrace.ToString());
                }

                

                //var req = ctx.Requests.Include("RequestDetails").Include("RequestDetails.Item").Where(r => r.RequestId == request.RequestId && r.Department.DepartmentId == request.Department.DepartmentId).SingleOrDefault();
                //req.Status = request.Status;
                //req.ApprovedBy = request.ApprovedBy;
                //ctx.Users.Attach(req.ApprovedBy);
                //ctx.Departments.Attach(request.Department);

                //if(req.Status == (int)RequestStatus.Approved)
                //{
                //    //get list of item ids
                //    Disbursement disbursement = new Disbursement();
                //    List<int> itemIds = req.RequestDetails.Select(rd => rd.Item.ItemId).ToList();
                //    List<DisbursementDetail> disbursementDetails = new List<DisbursementDetail>();

                //    //get the current stock items 
                //    IDictionary<int, Item> itemsDictionary = ctx.Items.Where(i => itemIds.Contains(i.ItemId)).ToDictionary(i => i.ItemId, i => i);

                //    foreach(var rD in req.RequestDetails)
                //    {
                //        Item i = itemsDictionary[rD.Item.ItemId];
                       
                //        if(i.Quantity != 0) {

                //            if (rD.Quantity > i.Quantity)
                //            {
                //                //requested amount is more than current stock
                //                disbursement.Status = (int)DisbursementStatus.Allocated;
                //                req.Status = (int)RequestStatus.PartiallyAllocated;
                //                DisbursementDetail dd = new DisbursementDetail()
                //                {
                //                    Item = i,
                //                    Quantity = i.Quantity
                //                };

                //                rD.DeliveredQuantity = i.Quantity;

                //                i.Quantity = 0;
                //                disbursementDetails.Add(dd);
                //            }
                //            else
                //            {
                //                DisbursementDetail dd = new DisbursementDetail()
                //                {
                //                    Item = i,
                //                    Quantity = rD.Quantity,
                //                };

                //                rD.DeliveredQuantity = rD.Quantity;

                //                i.Quantity = i.Quantity - rD.Quantity;
                //                disbursementDetails.Add(dd);
                //            }
                //        }
                        
                //    }

                //    if (disbursementDetails.Count > 0)
                //    {
                //        disbursement.DisbursementDetails = disbursementDetails;
                //        disbursement.Date = DateTime.Now.AddDays(-1);
                //        disbursement.Department = request.Department;
                //        disbursement.Request = req;
                //        ctx.Disbursements.Add(disbursement);
                //        ctx.DisbursementDetails.AddRange(disbursementDetails);

                //        if (req.Status != (int)RequestStatus.PartiallyAllocated)
                //        {
                //            req.Status = (int)RequestStatus.FullyAllocated;
                //        }
                //    }
                    
                //}
                //try
                //{
                //    ctx.SaveChanges();

                //}
                //catch (Exception e)
                //{
                //    Debug.WriteLine(e.InnerException.Message);
                //    Debug.WriteLine(e.InnerException.StackTrace.ToString());
                //}
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
                if(u.Rank == (int)UserRank.Clerk || u.Rank == (int)UserRank.Supervisor || u.Rank == (int)UserRank.Manager)
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


        //update request for delivered qty,status,etc.
        public static void UpdateRequestById(int requestId)
        {
            using(var ctx = new UniDBContext())
            {
                Request req = ctx.Requests.Include("RequestDetails").Include("RequestDetails.Item")
                    .Where(r => r.RequestId == requestId).SingleOrDefault();
                //delivered item amount dictionary 
                //item key and total delivered qty value
                Dictionary<int, int> deliveredItemDict = ctx.DisbursementDetails.Include("Disbursement").Include("Disbursement.Request").Include("Item")
                    .Where(dd => dd.Disbursement.Request.RequestId == requestId)
                    .GroupBy(dd => dd.Item.ItemId)
                    .Select(c => new
                    {
                        ItemId = c.Key,
                        Total = c.Sum(dd => dd.Quantity)
                    })
                    .ToDictionary(dd => dd.ItemId, dd => dd.Total);
                bool fulfilRequest = true;
                foreach(var rD in req.RequestDetails)
                {
                    if (deliveredItemDict.ContainsKey(rD.Item.ItemId))
                    {
                        int currentDeliveredQty = deliveredItemDict[rD.Item.ItemId];
                        if (rD.Quantity == currentDeliveredQty)
                        {
                            rD.DeliveredQuantity = currentDeliveredQty;
                        }
                        else
                        {
                            int temp = deliveredItemDict[rD.Item.ItemId];
                            int amountToIncrease = Math.Abs(rD.DeliveredQuantity - temp);
                            rD.DeliveredQuantity += amountToIncrease;
                            fulfilRequest = false;
                        }
                    }
                    
                    
                }

                if(fulfilRequest != true)
                {
                    req.Status = (int)RequestStatus.PartiallyDelivered;
                }else
                {
                    req.Status = (int)RequestStatus.FullyDelivered;
                }
                req.DisbursementStatus = (int)RequestRetrievalStatus.NotPrepared;
                ctx.SaveChanges();

            }
        }
                
        public static List<Request> getHistoricalRequestsByDepartment(int departmentId)
        {
            try
            {
                using (var ctx = new UniDBContext())
                {
                    List<Request> requests = ctx.Requests.Include("Requestor").Where(r => r.Requestor.Department.DepartmentId == departmentId && r.Status != (int)RequestStatus.Requested).ToList();

                    return requests;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }
               
        public static List<RetrievalItem> GetRequestedItemsByMonth(int deptId, int month)
        {
            try
            {
                using (var ctx = new UniDBContext())
                {
                    List<Request> requestList = ctx.Requests.Include("Department").Include("RequestDetails").Include("RequestDetails.Item")
                        .Where(r => r.Date.Year == DateTime.Now.Year && r.Date.Month == month && r.Status != (int)RequestStatus.Requested && r.Status != (int)RequestStatus.Rejected && r.Status != (int)RequestStatus.Cancelled)
                        .ToList();
                    if(deptId != 0)
                    {
                        requestList = requestList.Where(r => r.Department.DepartmentId == deptId).ToList();
                    }
                    Dictionary<int, int> itemInfo = new Dictionary<int, int>();

                    foreach (var r in requestList)
                    {
                        foreach (var rD in r.RequestDetails)
                        {
                            var requiredAmount = rD.Quantity;
                            if (itemInfo.ContainsKey(rD.Item.ItemId))
                            {
                                requiredAmount += itemInfo[rD.Item.ItemId];
                                itemInfo[rD.Item.ItemId] = requiredAmount;
                            }
                            else
                            {
                                itemInfo.Add(rD.Item.ItemId, requiredAmount);
                            }
                        }
                    }
            }
        }

        public static List<Request> GetUnFulfilledRequests()
        {
            using(var ctx = new UniDBContext())
            {
                List<Request> requests = ctx.Requests.Include("Department")
                    .Where(r => r.Status == (int)RequestStatus.Approved || r.Status == (int)RequestStatus.PartiallyDelivered)
                    .ToList();

                return requests;
            }
        }

                    List<int> itemIds = itemInfo.Keys.ToList();
                    Dictionary<int, Item> stockDict = ctx.Items.Where(i => itemIds.Contains(i.ItemId)).ToDictionary(i => i.ItemId);
                    List<RetrievalItem> retrievalItems = new List<RetrievalItem>();
                    foreach (var itemId in itemIds)
                    {
                        int requestedNumber = itemInfo[itemId];
                        Item i = stockDict[itemId];

                        RetrievalItem retrieval = new RetrievalItem()
                        {
                            AllocatedQuantity = requestedNumber,
                            Description = i.Description,
                            ItemId = i.ItemId,
                        };

                        retrievalItems.Add(retrieval);

                    }
                    return retrievalItems;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}