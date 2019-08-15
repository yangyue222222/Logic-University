using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebApplication1.DataAccessLayer;
using WebApplication1.Models;
using System.Diagnostics;

namespace WebApplication1.DAOs
{
    public class UserDao
    {
        public static User GetUser(User user)
        {
            using (var ctx = new UniDBContext())
            {
                User u = ctx.Users.Where(us => us.Username.Equals(user.Username) && us.Password.Equals(user.Password)).SingleOrDefault();
                if(u != null)
                {
                    ctx.Entry(u).Reference(s => s.Department).Load();
                }
                return u;
            }
        }

        public async static Task<User> GetRepresentative(int departmentId) {
            using(var ctx = new UniDBContext())
            {
                Department department = await ctx.Departments.Include("Representative").Where(d => d.DepartmentId == departmentId).FirstOrDefaultAsync<Department>();
                User u = department.Representative;
                return u;
            }
        }

        public async static Task<List<User>> GetAllEmployeesFromDepartment(int departmentId)
        {
            using (var ctx = new UniDBContext())
            {
                List<User> users = await ctx.Users.Where(u => u.Department.DepartmentId == departmentId).ToListAsync();
                return users;
            }
        }

        public async static Task<User> GetTemporaryHeadByDepartment(int departmentId)
        {
            using(var ctx = new UniDBContext())
            {
                User u = ctx.Users.Include("Department").Where(user => user.Department.DepartmentId == departmentId &&
                user.Rank == (int)UserRank.TemporaryHead).SingleOrDefault();

                return u;
            }
        }


        public static User GetUserProfile(User user)
        {
            using (var ctx = new UniDBContext())
            {
                User loggedInUser = ctx.Users.Include("Department").Where(u => u.UserId == user.UserId && u.Department.DepartmentId == user.Department.DepartmentId).SingleOrDefault();
                return loggedInUser;
            }
        }
    }
}