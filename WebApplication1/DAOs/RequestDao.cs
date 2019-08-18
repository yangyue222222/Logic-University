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
            }
            catch (Exception e)
            {
                throw new Exception("Internal Server Error");
            }
        }

        public static List<Request> getRequestsByDepartment(int departmentId)
        {
            try
            {
                using (var ctx = new UniDBContext())
                {
                    List<Request> requests = ctx.Requests.Include("Requestor").Where(r => r.Requestor.Department.DepartmentId == departmentId && r.Status == (int)RequestStatus.Requested).ToList();

                    return requests;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static List<RequestDetail> getRequestDetailsByRequestId(int id, int departmentId)
        {
            List<RequestDetail> details = null;
            using (var ctx = new UniDBContext())
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
            }
        }

        public static List<RetrievalItem> GetRequestDeailsByItems()
        {
            using (var ctx = new UniDBContext())
            {
                List<RetrievalItem> items = ctx.RequestDetails.Include("Request").Include("Item")
                    .Where(r =>
                        r.Request.Date.CompareTo(DateTime.Now) < 0 && r.Request.Status == (int)RequestStatus.Approved
                    ).GroupBy(r => new { r.Item.Description, r.Item.ItemId })
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

        public static void CancelRequestById(int requestId, User u)
        {
            using (var ctx = new UniDBContext())
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

        public static Request GetRequestById(int requestId, User u)
        {
            using (var ctx = new UniDBContext())
            {
                IQueryable<Request> query = ctx.Requests.Include("Requestor").Include("RequestDetails").Include("ApprovedBy").Include("Department").Include("RequestDetails.Item");
                if (u.Rank == (int)UserRank.Clerk || u.Rank == (int)UserRank.Supervisor || u.Rank == (int)UserRank.Manager)
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
            using (var ctx = new UniDBContext())
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
                foreach (var rD in req.RequestDetails)
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
                    else
                    {
                        fulfilRequest = false;
                    }


                }

                if (fulfilRequest != true)
                {
                    req.Status = (int)RequestStatus.PartiallyDelivered;
                }
                else
                {
                    req.Status = (int)RequestStatus.FullyDelivered;
                }
                req.DisbursementStatus = (int)RequestRetrievalStatus.NotPrepared;
                ctx.SaveChanges();

            }
        }

        public static List<Request> GetUnFulfilledRequests()
        {
            using (var ctx = new UniDBContext())
            {
                List<Request> requests = ctx.Requests.Include("Department")
                    .Where(r => r.Status == (int)RequestStatus.Approved || r.Status == (int)RequestStatus.PartiallyDelivered)
                    .ToList();

                return requests;
            }
        }

        //YANG Part

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
        //public static List<Request> GetUnFulfilledRequests()
        //{
        //    using (var ctx = new UniDBContext())
        //    {
        //        List<Request> requests = ctx.Requests.Include("Department")
        //            .Where(r => r.Status == (int)RequestStatus.Approved || r.Status == (int)RequestStatus.PartiallyDelivered)
        //            .ToList();

        //        return requests;
        //    }
        //}


        //key for item description,
        //another dict key for month and value for no of items requested 
        public static Dictionary<string, Dictionary<int, int>> GetRequestedItemNoByCategory(string category)
        {
            using (var ctx = new UniDBContext())
            {
                //description key ,
                Dictionary<string, List<RequestDetail>> rDDict = ctx.RequestDetails.Include("Request").Include("Item")
                    .Where(rd => (rd.Request.Status == (int)RequestStatus.Approved || rd.Request.Status == (int)RequestStatus.PartiallyDelivered ||
                    rd.Request.Status == (int)RequestStatus.FullyDelivered) && rd.Request.Date.Year == DateTime.Now.Year && rd.Item.Category == category)
                    .GroupBy(rd => rd.Item.Description)
                    .ToDictionary(cl => cl.Key, cl => cl.ToList());

                Dictionary<string, Dictionary<int, int>> itemInfo = new Dictionary<string, Dictionary<int, int>>();
                foreach (KeyValuePair<string, List<RequestDetail>> kV in rDDict)
                {
                    List<int> rdIds = kV.Value.Select(rd => rd.RequestDetailId).ToList();

                    Dictionary<int, int> monthAndItemAmount = ctx.RequestDetails.Include("Request")
                        .Where(rd => rdIds.Contains(rd.RequestDetailId))
                        .GroupBy(rd => rd.Request.Date.Month)
                        .ToDictionary(cl => cl.Key, cl => cl.Sum(rd => rd.Quantity));

                    itemInfo.Add(kV.Key, monthAndItemAmount);

                }

                return itemInfo;
            }
        }

        public static void UpdateRequestDisbursementStatus(int requestId)
        {
            using (var ctx = new UniDBContext())
            {
                Request req = ctx.Requests.Where(r => r.RequestId == requestId).SingleOrDefault();
                if (req != null)
                {
                    req.DisbursementStatus = (int)RequestRetrievalStatus.NotPrepared;
                    ctx.SaveChanges();
                }
            }
        }

        public static List<RequestDetail> GetUnfulfilledRequestDetails()
        {
            using (var ctx = new UniDBContext())
            {
                List<RequestDetail> details = ctx.RequestDetails.Include("Request").Include("Item")
                    .Where(rd => rd.Request.Status == (int)RequestStatus.Approved || rd.Request.Status == (int)RequestStatus.PartiallyDelivered)
                    .Where(rd => rd.Quantity - rd.DeliveredQuantity != 0)
                    .ToList();

                return details;

            }
        }

        public static List<RequestDetail> GetLatestRequisitionByDepartment(int departmentId)
        {
            using (var ctx = new UniDBContext())
            {
                Request request = ctx.Requests.Include("Department").Include("RequestDetails").Include("RequestDetails.Item")
                    .Where(r => r.Department.DepartmentId == departmentId && (r.Status == (int)RequestStatus.Approved || r.Status == (int)RequestStatus.PartiallyDelivered || r.Status == (int)RequestStatus.FullyDelivered))
                    .OrderByDescending(r => r.RequestId)
                    .FirstOrDefault();
                   
                if(request != null)
                {
                    return request.RequestDetails.ToList();
                }
                
                return null;


            }

        }

        public static Dictionary<string, List<RequestDetail>> GetRequestDetailsByAllDepartments() {
        
            using(var ctx = new UniDBContext())
            {
                Dictionary<string, List<RequestDetail>> detailDict = ctx.RequestDetails.Include("Request").Include("Request.Department").Include("Item")
                    .Where(rd => rd.Request.Status != (int)RequestStatus.Cancelled && rd.Request.Status != (int)RequestStatus.Rejected && rd.Request.Status != (int)RequestStatus.Requested && rd.Request.Date.Year == DateTime.Now.Year)
                    .GroupBy(rd => rd.Request.Department.DepartmentName)
                    .ToDictionary(cl => cl.Key, cl => cl.ToList());

                return detailDict;
            }
        }

    }
}