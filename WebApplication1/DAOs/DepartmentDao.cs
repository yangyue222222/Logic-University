using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.DataAccessLayer;
using WebApplication1.Models;

namespace WebApplication1.DAOs
{
    public class DepartmentDao
    {
        public static void UpdateDepartmentRepresentative(int userId,int departmentId)
        {
            using (var ctx = new UniDBContext())
            {
                User u = new User()
                {
                    UserId = userId
                };
                Department d = ctx.Departments.Where(de => de.DepartmentId == departmentId).SingleOrDefault();
                d.Representative = u;
                ctx.Users.Attach(u);
                ctx.SaveChanges();
            }
        }
    }
}