using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.Models;

namespace WebApplication1.DataAccessLayer
{
    public class SetRepresentatives
    {
        public void setReps()
        {
            using (var ctx = new UniDBContext())
            {
                User e2 = ctx.Users.Where(u => u.Username == "cse2").FirstOrDefault();
                Department csd = ctx.Departments.Where(d => d.DepartmentName == "CS").FirstOrDefault();
                csd.Representative = e2;
                User e4 = ctx.Users.Where(u => u.Username == "enge2").FirstOrDefault();
                Department engd = ctx.Departments.Where(d => d.DepartmentName == "English").FirstOrDefault();
                engd.Representative = e4;
                ctx.SaveChanges();
            }
        }
    }
}