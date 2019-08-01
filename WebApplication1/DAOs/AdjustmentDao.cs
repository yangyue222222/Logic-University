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
        public static List<AdjustmentDetail> InsertAdjustment(List<AdjustmentDetail> details,User requestor)
        {
            using(var ctx = new UniDBContext())
            {
                Adjustment adjustment = new Adjustment();
                ctx.Users.Attach(requestor);
                foreach(var d in details)
                {
                    ctx.Items.Attach(d.Item);
                }
                ctx.AdjustmentDetails.AddRange(details);
                adjustment.AdjustmentDetails = details;
                adjustment.Requestor = requestor;
                adjustment.RaisedTime = DateTime.Now;
                ctx.Adjustments.Add(adjustment);
                ctx.SaveChanges();
                return details;
            }
        }
    }
}