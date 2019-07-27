using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.DataAccessLayer;
using WebApplication1.Models;

namespace WebApplication1.DAOs
{
    public class UserDao
    {
        public static User GetUser(User user)
        {
            using (var ctx = new UniDBContext())
            {
                User u = ctx.Users.Where(us => us.Username.Equals(user.Username) && us.Password.Equals(user.Password)).SingleOrDefault();
                ctx.Entry(u).Reference(s => s.Department).Load();
                return u;
            }
        }
    }
}