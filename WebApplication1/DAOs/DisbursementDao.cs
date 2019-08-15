using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.DataAccessLayer;
using WebApplication1.Models;
using System.Data.Entity;
using System.Diagnostics;

namespace WebApplication1.DAOs
{
    public class DisbursementDao
    {
        public static List<RetrievalItem> GetAllItemsForRetrieval()
        {
            using(var ctx = new UniDBContext())
            {

                List<int> requestStatus = new List<int>()
                {
                    (int)RequestStatus.Approved,
                    (int)RequestStatus.PartiallyDelivered
                };

                //key is item id and value is total request item amount

                List<Request> requestList = ctx.Requests.Include("RequestDetails").Include("RequestDetails.Item")
                    .Where(r => requestStatus.Contains(r.Status) && r.DisbursementStatus == (int) RequestRetrievalStatus.NotPrepared && 
                        DbFunctions.TruncateTime(r.Date) < DbFunctions.TruncateTime(DateTime.Today))
                    .ToList();

                //item id and number of item needed
                Dictionary<int, int> itemInfo = new Dictionary<int, int>();

                foreach(var r in requestList)
                {
                    foreach(var rD in r.RequestDetails)
                    {
                        var requiredAmount = rD.Quantity - rD.DeliveredQuantity;
                        if(requiredAmount != 0)
                        {
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
                    
                //Dictionary<int, int> requestDetails = ctx.RequestDetails.Include("Request").Include("Item")
                //    .Where(r => requestStatus.Contains(r.Request.Status) && DbFunctions.TruncateTime(r.Request.Date) < DbFunctions.TruncateTime(DateTime.Today))
                //    .GroupBy(r => r.Item.ItemId)
                //    .Select(cl => new
                //    {
                //        key = cl.Key,
                //        total = cl.Sum(rd => rd.Quantity)
                //    })
                //    .ToDictionary(c => c.key, c => c.total);



                //check with the stock number and required amount from request
                List<int> itemIds = itemInfo.Keys.ToList();
                //key is item id and value is stock item object
                Dictionary<int, Item> stockDict = ctx.Items.Where(i => itemIds.Contains(i.ItemId)).ToDictionary(i => i.ItemId);
                List<RetrievalItem> retrievalItems = new List<RetrievalItem>();
                foreach(var itemId in itemIds)
                {
                    int requestedNumber = itemInfo[itemId];
                    Item i = stockDict[itemId];
                    int availableNumber = i.Quantity;
                    if(availableNumber != 0)
                    {
                        int allocatedQty = 0;
                        if (requestedNumber >= availableNumber)
                        {
                            allocatedQty = availableNumber;
                        }
                        else
                        {
                            allocatedQty = requestedNumber;
                        }
                        RetrievalItem retrieval = new RetrievalItem()
                        {
                            AllocatedQuantity = allocatedQty,
                            Description = i.Description,
                            ItemId = i.ItemId,
                            StockQuantity = i.Quantity
                        };

                        retrievalItems.Add(retrieval);
                    }
                    
                }
                //List<RetrievalItem> items = ctx.DisbursementDetails.Include("Disbursement").Include("Item")
                //    .Where(d => d.Disbursement.Status == (int)DisbursementStatus.Allocated && DbFunctions.TruncateTime(d.Disbursement.Date) < DbFunctions.TruncateTime(DateTime.Today))
                //    .GroupBy(d => new { ItemId = d.Item.ItemId, Description = d.Item.Description })
                //    .Select(x => new RetrievalItem {Description = x.Key.Description, ItemId = x.Key.ItemId, AllocatedQuantity = x.Sum(y => y.Quantity) }).ToList();
                return retrievalItems;
            }

        }

        //public static void UpdateDisbursementsStatus()
        //{
        //    using(var ctx = new UniDBContext())
        //    {
        //        List<Disbursement> disbursements = ctx.Disbursements.Include("DisbursementDetails").Include("DisbursementDetails.Item")
        //            .Where(d => d.Status == (int)DisbursementStatus.Allocated && DbFunctions.TruncateTime(d.Date) < DbFunctions.TruncateTime(DateTime.Today))
        //            .ToList();

        //        if(disbursements != null)
        //        {
        //            foreach(var d in disbursements)
        //            {
        //                d.Status = (int)DisbursementStatus.Prepared;
        //            }

        //            ctx.SaveChanges();
        //        }
        //    }
        //}

        public static void GenerateDisbursements (List<RetrievalItem> retrievalItems)
        {
            using(var ctx = new UniDBContext())
            {
                List<int> requestStatus = new List<int>()
                {
                    (int)RequestStatus.Approved,
                    (int)RequestStatus.PartiallyDelivered
                };


                List<int> itemIds = retrievalItems.Select(r => r.ItemId).ToList();
                //item id key and its corresponding item
                Dictionary<int, Item> itemDict = ctx.Items.Where(i => itemIds.Contains(i.ItemId)).ToDictionary(i => i.ItemId);

                //get list of request which contins the requestdetail of item ids retrieved
                Dictionary<int, List<RequestDetail>> requestDetails = ctx.RequestDetails.Include("Request").Include("Item")
                    .Where(rd => requestStatus.Contains(rd.Request.Status) && DbFunctions.TruncateTime(rd.Request.Date) < DbFunctions.TruncateTime(DateTime.Today)
                            && itemIds.Contains(rd.Item.ItemId))
                    .GroupBy(rd => rd.Item.ItemId)
                    .ToDictionary(c => c.Key,c => c.ToList());

                foreach(var k in requestDetails.Keys)
                {
                    foreach(var detail in requestDetails[k])
                    {
                        Debug.WriteLine("Item Id {0} and Item Amount is {1}", detail.Item.ItemId, detail.Quantity);
                    }
                }



                //key of request and disbursementdetail list for generating disbursement
                Dictionary<int, List<DisbursementDetail>> ddDict = new Dictionary<int, List<DisbursementDetail>>();


                foreach(var itemId in itemIds)
                {
                    Item i = itemDict[itemId];
                    var rDList = requestDetails[itemId];

                    foreach(var rd in rDList)
                    {
                        var allocatedAmount = 0;
                        var requiredAmtForEachReq = rd.Quantity - rd.DeliveredQuantity;
                        if (i.Quantity > requiredAmtForEachReq)
                        {
                            allocatedAmount = requiredAmtForEachReq;
                            i.Quantity = i.Quantity - requiredAmtForEachReq;
                        }else if(i.Quantity == requiredAmtForEachReq)
                        {
                            allocatedAmount = i.Quantity;
                            i.Quantity = 0;
                        }else
                        {
                            allocatedAmount = i.Quantity;
                            i.Quantity = 0;
                        }

                        if(allocatedAmount != 0)
                        {
                            DisbursementDetail dd = new DisbursementDetail()
                            {
                                Item = i,
                                Quantity = allocatedAmount
                            };

                            ctx.DisbursementDetails.Add(dd);

                            Request r = ctx.RequestDetails.Where(rDetail => rDetail.RequestDetailId == rd.RequestDetailId).Include("Request").Select(rDetail => rDetail.Request).SingleOrDefault();
                            r.DisbursementStatus = (int)RequestRetrievalStatus.Prepared;
                            if (ddDict.ContainsKey(r.RequestId))
                            {
                                ddDict[r.RequestId].Add(dd);
                            }
                            else
                            {
                                ddDict[r.RequestId] = new List<DisbursementDetail>();
                                ddDict[r.RequestId].Add(dd);
                            }
                        }
                        

                        if(i.Quantity == 0)
                        {
                            break;
                        }

                    }

                }

                ctx.SaveChanges();


                foreach(KeyValuePair<int,List<DisbursementDetail>> k in ddDict)
                {
                    Request r = ctx.Requests.Include("Department").Where(req => req.RequestId == k.Key).SingleOrDefault();
                    r.DisbursementStatus = (int)RequestRetrievalStatus.Prepared;
                    Disbursement d = new Disbursement()
                    {
                        DisbursementDetails = k.Value,
                        Date = DateTime.Now,
                        Request = r,
                        Department = r.Department
                    };
                    ctx.Disbursements.Add(d);
                }

                ctx.SaveChanges();

            }
        }




        //public static void UpdateDisbursementDetailsForItemMissing(List<AdjustmentDetail> adjustmentDetails)
        //{
        //    List<int> itemIds = adjustmentDetails.Select(ad => ad.Item.ItemId).ToList();
        //    using(var ctx = new UniDBContext())
        //    {
               
        //        Dictionary<int, List<DisbursementDetail>> details = ctx.DisbursementDetails.Include("Disbursement").Include("Item").Include("Disbursement.Request")
        //            .Where(d => itemIds.Contains(d.Item.ItemId) && (d.Disbursement.Request.Status == (int)RequestStatus.FullyAllocated || d.Disbursement.Request.Status == (int)RequestStatus.PartiallyAllocated))
        //            .GroupBy(d => d.Item.ItemId)
        //            .Select(d => new { ItemId = d.Key, DisbursementDetails = d.ToList() })
        //            .ToDictionary(d => d.ItemId, d => d.DisbursementDetails);
        //        if (details != null)
        //        {
        //            List<int> dibursementDetailIds = new List<int>();

        //            foreach(var aD in adjustmentDetails)
        //            {
        //                int mismatch = aD.Count;
        //                List<DisbursementDetail> dDetails = details[aD.Item.ItemId];
        //                dDetails = dDetails.OrderByDescending(d => d.DisbursementDetailId).ToList();
                        
        //                foreach(var d in dDetails)
        //                {
        //                    dibursementDetailIds.Add(d.DisbursementDetailId);
        //                    Debug.WriteLine(d.DisbursementDetailId);
        //                    if (mismatch > d.Quantity)
        //                    {
        //                        mismatch = mismatch - d.Quantity;
        //                        ctx.DisbursementDetails.Remove(d);
        //                    }else
        //                    {
        //                        if(mismatch == d.Quantity)
        //                        {
        //                            ctx.DisbursementDetails.Remove(d);
        //                        }else
        //                        {
        //                            d.Quantity = d.Quantity - mismatch;
        //                        }

        //                        mismatch = 0;
        //                    }

        //                    if (mismatch == 0)
        //                    {
        //                        break;
        //                    }

        //                }

        //            }
        //            List<Request> requests = ctx.DisbursementDetails.Include("Disbursement").Include("Disbursement.Request")
        //                .Where(d => dibursementDetailIds.Contains(d.DisbursementDetailId)).Select(d => d.Disbursement.Request).Distinct().ToList();
        //            foreach(var r in requests)
        //            {
        //                if(r.Status == (int)RequestStatus.FullyAllocated)
        //                {
        //                    r.Status = (int)RequestStatus.PartiallyAllocated;
        //                }
        //            }

        //            ctx.SaveChanges();   
        //        }

        //        List<Disbursement> disburse = ctx.Disbursements.Where(d => d.Status == (int)DisbursementStatus.Allocated).ToList();
        //        foreach(var d in disburse)
        //        {
        //            d.Status = (int)DisbursementStatus.Prepared;
        //        }

        //        ctx.SaveChanges();
        //    }
        //}

        public static List<Disbursement> GetPreparedDisbursements(int userId)
        {
            using(var ctx = new UniDBContext())
            {
                List<Disbursement> disbursements = ctx.Disbursements.Include("Request").Include("Department").Include("Department.PickupPoint").Include("Department.PickupPoint.StoreClerk")
                    .Where(d => d.Status == (int)DisbursementStatus.Prepared && d.Department.PickupPoint != null)
                    .Where(d => d.Department.PickupPoint.StoreClerk.UserId == userId)
                    .ToList();

                return disbursements;
            }
        }
        public static List<Disbursement> GetPreparedDisbursementsForMobile(int userId)
        {
            using (var ctx = new UniDBContext())
            {
                List<Disbursement> disbursements = ctx.Disbursements.Include("Department").Include("Department.Representative").Include("DisbursementDetails").Include("DisbursementDetails.Item")
                    .Where(d => d.Status == (int)DisbursementStatus.Prepared && d.Department.PickupPoint != null)
                    .Where(d => d.Department.PickupPoint.StoreClerk.UserId == userId)
                    .ToList();

                return disbursements;
            }
        }
        public static Disbursement GetDisbursementByDetailId(int id)
        {
            using (var ctx = new UniDBContext())
            {
                DisbursementDetail dd = ctx.DisbursementDetails.Include("Disbursement").Include("Item")
                    .Where(d => d.DisbursementDetailId == id).FirstOrDefault();
                Disbursement disbursement = dd.Disbursement;
                return disbursement;
            }
        }

        public static List<Disbursement> GetPreparedDisbursements(int departmentId, int userId)
        {
            using(var ctx = new UniDBContext())
            {
                List<Disbursement> disbursements = ctx.Disbursements.Include("Department").Include("DisbursementDetails").Include("DisbursementDetails.Item")
                    .Include("Department.PickupPoint").Include("Department.Representative")
                    .Where(d => d.Department.DepartmentId == departmentId && d.Department.Representative != null)
                    .Where(d => d.Status == (int)DisbursementStatus.Prepared)
                    .Where(d => d.Department.Representative.UserId == userId).ToList();

                return disbursements;
                    
            }
        }

        public static List<Disbursement> GetAllDisbursementsByDepartment(int departmentId)
        {
            using (var ctx = new UniDBContext())
            {
                List<Disbursement> disbursements = ctx.Disbursements.Include("Request").Include("Department").Include("ApprovedBy")
                    .OrderByDescending(d => d.DisbursementId)
                    .Where(d => d.Department.DepartmentId == departmentId).ToList();

                return disbursements;
            }
        }


        public static Disbursement GetDisbursement(int id)
        {
            using(var ctx = new UniDBContext())
            {
                Disbursement disbursement = ctx.Disbursements.Include("Department").Include("Department.Representative").Include("DisbursementDetails").Include("DisbursementDetails.Item")
                    .Include("Request")
                    .Where(d => d.Status == (int)DisbursementStatus.Prepared && d.DisbursementId == id).SingleOrDefault();
                Debug.WriteLine("Delivery Department {0}", disbursement.Department.DepartmentName);
                return disbursement;
            }
        }

        public static Dictionary<int,DisbursementDetail> GetDisbursementDetailDictByDisbursementId(int disbursementId)
        {
            using(var ctx = new UniDBContext())
            {
                Dictionary<int, DisbursementDetail> dDict = ctx.DisbursementDetails.Include("Disbursement").Where(dd => dd.Disbursement.DisbursementId == disbursementId)
                    .ToDictionary(dd => dd.DisbursementDetailId);
                return dDict;
            }
        }


        public static Disbursement DeliverDisbursement(int disbursementId,List<DisbursementDetail> details)
        {
            using(var ctx = new UniDBContext())
            {
                List<int> detailIds = details.Select(dd => dd.DisbursementDetailId).ToList();
                //disbursementdetail id and its corresponding object
                Dictionary<int, DisbursementDetail> ddDict = ctx.DisbursementDetails.Include("Disbursement")
                    .Where(dd => detailIds.Contains(dd.DisbursementDetailId) && dd.Disbursement.DisbursementId == disbursementId)
                    .ToDictionary(dd => dd.DisbursementDetailId);

                foreach(var dd in details)
                {
                    DisbursementDetail dis = ddDict[dd.DisbursementDetailId];
                    if(dd.Quantity == 0)
                    {
                        ctx.DisbursementDetails.Remove(dis);
                    }else
                    {
                        //update actual delivered qty to the row in the database
                        dis.Quantity = dd.Quantity;
                    }
                }

                ctx.SaveChanges();

                Disbursement d = ctx.Disbursements.Include("Request").Include("DisbursementDetails").Where(dis => dis.DisbursementId == disbursementId).SingleOrDefault();
                if(d.DisbursementDetails.Count == 0)
                {
                    ctx.Disbursements.Remove(d);
                }else
                {
                    d.Status = (int)DisbursementStatus.Delivered;
                    d.Date = DateTime.Now;
                }

                ctx.SaveChanges();
                return d;
            }
        }

        public static List<Disbursement> GetDisbursementsByDepartmentAndMonth(int userId, int departmentId, int month, int disbursementStatus)
        {
            using (var ctx = new UniDBContext())
            {
                List<Disbursement> disbursements = ctx.Disbursements.Include("Department").Include("Department.Representative").Include("DisbursementDetails").Include("DisbursementDetails.Item")
                    .Where(d => d.Department.DepartmentId == departmentId && d.Date.Year == DateTime.Now.Year && d.Date.Month == month 
                    && d.Status == disbursementStatus && d.Department.Representative != null)
                    .Where(d => d.Department.Representative.UserId == userId)
                    .ToList();

                return disbursements;
            }
        }

        public static List<Disbursement> GetDisbursementsByDepartmentAndMonth(int departmentId,int month,int disbursementStatus) {
            using(var ctx = new UniDBContext())
            {
                List<Disbursement> disbursements = ctx.Disbursements.Include("Department").Include("DisbursementDetails").Include("DisbursementDetails.Item")
                    .Where(d => d.Department.DepartmentId == departmentId && d.Date.Year == DateTime.Now.Year && d.Date.Month == month && d.Status == disbursementStatus)
                    .ToList();

                return disbursements;
            }
        }

        public static void GenerateInvoiceByDepartmentAndMonth(int departmentId, int month)
        {
            using(var ctx = new UniDBContext())
            {
                List<Disbursement> disbursements = ctx.Disbursements.Include("Department")
                    .Where(d => d.Department.DepartmentId == departmentId && d.Date.Year == DateTime.Now.Year && d.Date.Month == month && d.Status == (int)DisbursementStatus.Delivered)
                    .ToList();

                if(disbursements != null)
                {
                    foreach(var d in disbursements)
                    {
                        d.Status = (int)DisbursementStatus.InvoiceGenerated;
                    }
                    ctx.SaveChanges();
                }
            }
        }

        public static Disbursement GetDeliveredDisbursementById(int disbursementId)
        {
            using(var ctx = new UniDBContext())
            {
                Disbursement disbursement = ctx.Disbursements.Include("DisbursementDetails").Include("DisbursementDetails.Item").Include("ApprovedBy")
                    .Include("Department")
                    .Where(d => d.DisbursementId == disbursementId && d.Status == (int)DisbursementStatus.Approved)
                    .SingleOrDefault();
                return disbursement;
            }
        }

        public static void ApproveDisubrsementById(User u , int disbursementId)
        {
            using(var ctx = new UniDBContext())
            {
                Disbursement dis = ctx.Disbursements.Include("Department")
                    .Where(d => d.Department.DepartmentId == u.Department.DepartmentId && d.DisbursementId == disbursementId && d.Status == (int)DisbursementStatus.Delivered)
                    .SingleOrDefault();
                User user = new User()
                {
                    UserId = u.UserId
                };
                dis.ApprovedBy = user;
                dis.Date = DateTime.Now;
                dis.Status = (int)DisbursementStatus.Approved;
                ctx.Users.Attach(user);
                ctx.SaveChanges();
            }
        }

        public static Disbursement GetDisbursementDetailById(int disbursementId)
        {
            using(var ctx = new UniDBContext())
            {
                Disbursement dis = ctx.Disbursements.Include("Department").Include("DisbursementDetails").Include("DisbursementDetails.Item")
                    .Include("ApprovedBy")
                    .Where(d => d.DisbursementId == disbursementId).SingleOrDefault();
                return dis;
            }
        }


    }
}