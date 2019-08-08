﻿using System;
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

        public static void DelegateAuthority(int departmentId,int userId)
        {
            using(var ctx = new UniDBContext())
            {
                //check if there is an active temporary head or not
                User currentUser = ctx.Users.Include("Department").Where(user => user.Department.DepartmentId == departmentId && user.Rank == (int)UserRank.TemporaryHead)
                    .SingleOrDefault();

                if(currentUser == null)
                {
                    User u = ctx.Users.Include("Department").Where(us => us.Department.DepartmentId == departmentId && us.UserId == userId)
                    .SingleOrDefault();
                    if (u != null)
                    {
                        u.Rank = (int)UserRank.TemporaryHead;
                        ctx.SaveChanges();
                    }
                }

            }
        }

        public static List<Department> GetAllDepartments(){
            using(var ctx = new UniDBContext())
            {
                List<Department> departments = ctx.Departments.ToList();
                return departments;
            }
        }
    }
}