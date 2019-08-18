using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.Models;
using WebApplication1.DataAccessLayer;

namespace WebApplication1.DAOs
{
    public class AdjustmentDao
    {
        public static Adjustment InsertAdjustment(List<AdjustmentDetail> details, User requestor)
        {
            using (var ctx = new UniDBContext())
            {
                Adjustment adjustment = new Adjustment();
                ctx.Users.Attach(requestor);
                foreach (var d in details)
                {
                    ctx.Items.Attach(d.Item);
                }
                ctx.AdjustmentDetails.AddRange(details);
                adjustment.AdjustmentDetails = details;
                adjustment.Requestor = requestor;
                adjustment.RaisedTime = DateTime.Now;
                adjustment.Status = (int)AdjustmentStatus.Raised;
                ctx.Adjustments.Add(adjustment);
                ctx.SaveChanges();
                return adjustment;
            }
        }

        public static void CalculateAdjustmentCost(Adjustment adjustment)
        {
            using (var ctx = new UniDBContext())
            {
                Adjustment ad = ctx.Adjustments.Include("AdjustmentDetails").Include("AdjustmentDetails.Item")
                    .Where(adj => adj.AdjustmentId == adjustment.AdjustmentId).SingleOrDefault();

                int total = 0;
                foreach(var add in ad.AdjustmentDetails)
                {
                    total += (add.Item.Price * add.Count);
                }

                ad.Total = total;

                ctx.SaveChanges();
            }
        }

        public static List<Adjustment> GetAllAdjustments(int userRank)
        {
            using(var ctx = new UniDBContext())
            {
                IQueryable<Adjustment> adjustQuery = null;
                List<Adjustment> adjustments = null;
                switch (userRank)
                {
                    case (int)UserRank.Manager:
                        adjustQuery = ctx.Adjustments.Include("Requestor").Where(ad => ad.Total >= 250);
                        break;
                    case (int)UserRank.Supervisor:
                        adjustQuery = ctx.Adjustments.Include("Requestor").Where(ad => ad.Total > 0 && ad.Total < 250);
                        break;
                }
                if(adjustQuery != null)
                {
                    adjustments = adjustQuery.OrderByDescending(ad => ad.AdjustmentId).ToList();
                }
                return adjustments;
            }
        }

        public static Adjustment GetAdjustmentDetailById(int adjustmentId)
        {
            using(var ctx = new UniDBContext())
            {
                Adjustment adj = ctx.Adjustments.Include("Requestor").Include("AdjustmentDetails").Include("AdjustmentDetails.Item")
                    .Where(ad => ad.AdjustmentId == adjustmentId).SingleOrDefault();
                return adj;
            }
        }

        public static void UpdateAdjustmentStatus(int adjustmentId,User u)
        {
            using(var ctx = new UniDBContext())
            {
                Adjustment adj = ctx.Adjustments.Where(ad => ad.AdjustmentId == adjustmentId).SingleOrDefault();
                adj.Status = (int)AdjustmentStatus.Approved;
                adj.ApprovedBy = u;
                ctx.Users.Attach(u);
                ctx.SaveChanges();
            }
        }

        public static List<Adjustment> GetAllAdjustmentsByClerk(int userId,int departmentId)
        {
            using(var ctx = new UniDBContext())
            {
                List<Adjustment> adjustments = ctx.Adjustments.Include("Requestor").Include("Requestor.Department")
                    .Where(ad => ad.Requestor.UserId == userId && ad.Requestor.Department.DepartmentId == departmentId)
                    .ToList();

                return adjustments;
            }
        }

        public static List<Adjustment> GetAllPendingAdjustments(int userId)
        {
            using(var ctx = new UniDBContext())
            {
                User user = ctx.Users.
                    Where(u => u.UserId == userId && (u.Rank == (int)UserRank.Manager || u.Rank == (int)UserRank.Supervisor))
                    .SingleOrDefault();
                if(user != null)
                {
                    IQueryable<Adjustment> adjustQuery = null;
                    List<Adjustment> adjustments = null;
                    switch (user.Rank)
                    {
                        case (int)UserRank.Manager:
                            adjustQuery = ctx.Adjustments.Include("Requestor")
                                .Where(ad => ad.Total >= 250 && ad.Status == (int)AdjustmentStatus.Raised);
                                break;
                        case (int)UserRank.Supervisor:
                            adjustQuery = ctx.Adjustments.Include("Requestor")
                                .Where(ad => ad.Total > 0 && ad.Total < 250 && ad.Status == (int)AdjustmentStatus.Raised);
                                break;
                    }

                    adjustments = adjustQuery.OrderByDescending(ad => ad.AdjustmentId).ToList();
                    return adjustments;
                }

                return null;
            }
        }

    }
}