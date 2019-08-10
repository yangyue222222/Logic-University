using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.Models;

namespace WebApplication1.DataAccessLayer
{
    public class PopulatePickUpPoint
    {
        public static void PopulatePoints(UniDBContext ctx)
        {
            PickUpPoint p1 = new PickUpPoint()
            {
                Location = "Stationery Store - Administration Building",
                PickUpTime = "9:30 AM"
            };

            PickUpPoint p2 = new PickUpPoint()
            {
                Location = "Management school",
                PickUpTime = "11:00 AM"
            };

            PickUpPoint p3 = new PickUpPoint()
            {
                Location = "Medical School",
                PickUpTime = "9:30 AM"
            };

            PickUpPoint p4 = new PickUpPoint()
            {
                Location = "Engineering School",
                PickUpTime = "11:00 AM"
            };

            PickUpPoint p5 = new PickUpPoint()
            {
                Location = "Science School",
                PickUpTime = "9:30 AM"
            };

            PickUpPoint p6 = new PickUpPoint()
            {
                Location = "University Hostpital",
                PickUpTime = "11:00 AM"
            };


            ctx.PickUpPoints.Add(p1);
            ctx.PickUpPoints.Add(p2);
            ctx.PickUpPoints.Add(p3);
            ctx.PickUpPoints.Add(p4);
            ctx.PickUpPoints.Add(p5);
            ctx.PickUpPoints.Add(p6);
            ctx.SaveChanges();
        }
    }
}