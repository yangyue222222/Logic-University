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
                List<RetrievalItem> items = ctx.DisbursementDetails.Include("Disbursement").Include("Item")
                    .Where(d => d.Disbursement.Status == (int)DisbursementStatus.Allocated && DbFunctions.TruncateTime(d.Disbursement.Date) < DbFunctions.TruncateTime(DateTime.Today))
                    .GroupBy(d => new { ItemId = d.Item.ItemId, Description = d.Item.Description })
                    .Select(x => new RetrievalItem {Description = x.Key.Description, ItemId = x.Key.ItemId, AllocatedQuantity = x.Sum(y => y.Quantity) }).ToList();
                return items;
            }

        }

        public static void UpdateDisbursementsStatus()
        {
            using(var ctx = new UniDBContext())
            {
                List<Disbursement> disbursements = ctx.Disbursements.Include("DisbursementDetails").Include("DisbursementDetails.Item")
                    .Where(d => d.Status == (int)DisbursementStatus.Allocated && DbFunctions.TruncateTime(d.Date) < DbFunctions.TruncateTime(DateTime.Today))
                    .ToList();

                if(disbursements != null)
                {
                    foreach(var d in disbursements)
                    {
                        d.Status = (int)DisbursementStatus.Prepared;
                    }

                    ctx.SaveChanges();
                }
            }
        }


        public static void UpdateDisbursementDetailsForItemMissing(List<AdjustmentDetail> adjustmentDetails)
        {
            List<int> itemIds = adjustmentDetails.Select(ad => ad.Item.ItemId).ToList();
            using(var ctx = new UniDBContext())
            {
               
                Dictionary<int, List<DisbursementDetail>> details = ctx.DisbursementDetails.Include("Disbursement").Include("Item").Include("Disbursement.Request")
                    .Where(d => itemIds.Contains(d.Item.ItemId) && (d.Disbursement.Request.Status == (int)RequestStatus.FullyAllocated || d.Disbursement.Request.Status == (int)RequestStatus.PartiallyAllocated))
                    .GroupBy(d => d.Item.ItemId)
                    .Select(d => new { ItemId = d.Key, DisbursementDetails = d.ToList() })
                    .ToDictionary(d => d.ItemId, d => d.DisbursementDetails);
                if (details != null)
                {
                    List<int> dibursementDetailIds = new List<int>();

                    foreach(var aD in adjustmentDetails)
                    {
                        int mismatch = aD.Count;
                        List<DisbursementDetail> dDetails = details[aD.Item.ItemId];
                        dDetails = dDetails.OrderByDescending(d => d.DisbursementDetailId).ToList();
                        
                        foreach(var d in dDetails)
                        {
                            dibursementDetailIds.Add(d.DisbursementDetailId);
                            Debug.WriteLine(d.DisbursementDetailId);
                            if (mismatch > d.Quantity)
                            {
                                mismatch = mismatch - d.Quantity;
                                ctx.DisbursementDetails.Remove(d);
                            }else
                            {
                                if(mismatch == d.Quantity)
                                {
                                    ctx.DisbursementDetails.Remove(d);
                                }else
                                {
                                    d.Quantity = d.Quantity - mismatch;
                                }

                                mismatch = 0;
                            }

                            if (mismatch == 0)
                            {
                                break;
                            }

                        }

                    }
                    List<Request> requests = ctx.DisbursementDetails.Include("Disbursement").Include("Disbursement.Request")
                        .Where(d => dibursementDetailIds.Contains(d.DisbursementDetailId)).Select(d => d.Disbursement.Request).Distinct().ToList();
                    foreach(var r in requests)
                    {
                        if(r.Status == (int)RequestStatus.FullyAllocated)
                        {
                            r.Status = (int)RequestStatus.PartiallyAllocated;
                        }
                    }

                    ctx.SaveChanges();   
                }

                List<Disbursement> disburse = ctx.Disbursements.Where(d => d.Status == (int)DisbursementStatus.Allocated).ToList();
                foreach(var d in disburse)
                {
                    d.Status = (int)DisbursementStatus.Prepared;
                }

                ctx.SaveChanges();
            }
        }

        public static List<Disbursement> GetPreparedDisbursements()
        {
            using(var ctx = new UniDBContext())
            {
                List<Disbursement> disbursements = ctx.Disbursements.Include("Request").Include("Department")
                    .Where(d => d.Status == (int)DisbursementStatus.Prepared).ToList();

                return disbursements;
            }
        }

        public static Disbursement GetDisbursement(int id)
        {
            using(var ctx = new UniDBContext())
            {
                Disbursement disbursement = ctx.Disbursements.Include("Department").Include("DisbursementDetails").Include("DisbursementDetails.Item")
                    .Where(d => d.Status == (int)DisbursementStatus.Prepared && d.DisbursementId == id).SingleOrDefault();
                return disbursement;
            }
        }


    }
}