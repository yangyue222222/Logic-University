using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.Models;

namespace WebApplication1.DataAccessLayer
{
    public class PopulateUser
    {
        public void populateUsers(UniDBContext ctx)
        {
            ICollection<User> users = new List<User>();
            User e1 = new User()
            {
                Name = "CS Employee 1",
                Username = "cse1",
                Password = "cse1",
                Rank = (int)UserRank.Employee
            };

            User e2 = new User()
            {
                Name = "CS Employee 1",
                Username = "cse2",
                Password = "cse2",
                Rank = (int)UserRank.Employee
            };

            User csHead = new User()
            {
                Name = "CS Head",
                Username = "cshead",
                Password = "cshead",
                Rank = (int)UserRank.Head
            };


            User e3 = new User()
            {
                Name = "English Employee 1",
                Username = "enge1",
                Password = "enge1",
                Rank = (int)UserRank.Employee
            };

            User e4 = new User()
            {
                Name = "English Employee 1",
                Username = "enge1",
                Password = "enge2",
                Rank = (int)UserRank.Employee
            };

            User engHead = new User()
            {
                Name = "English Head",
                Username = "enghead",
                Password = "enghead",
                Rank = (int)UserRank.Head
            };

            User storeClerk = new User()
            {
                Name = "Store Clerk",
                Username = "storeclerk",
                Password = "storeclerk",
                Rank = (int)UserRank.Clerk
            };

            User storeManager = new User()
            {
                Name = "Store Manager",
                Username = "storemanager",
                Password = "storemanager",
                Rank = (int)UserRank.Manager
            };

            User storeSupervisor = new User()
            {
                Name = "Store Clerk",
                Username = "storesupervisor",
                Password = "storesupervisor",
                Rank = (int)UserRank.Supervisor
            };

            Department csd = new Department()
            {
                DepartmentName = "CS",
            };

            Department engd = new Department()
            {
                DepartmentName = "English",
            };

            Department stored = new Department()
            {
                DepartmentName = "Store",
            };

            e1.Department = csd;
            e2.Department = csd;
            csHead.Department = csd;

            e3.Department = engd;
            e4.Department = engd;
            engHead.Department = engd;

            storeManager.Department = stored;
            storeClerk.Department = stored;
            storeSupervisor.Department = stored;

            ctx.Users.Add(e1);
            ctx.Users.Add(e2);
            ctx.Users.Add(csHead);

            ctx.Departments.Add(csd);

            ctx.Users.Add(e3);
            ctx.Users.Add(e4);
            ctx.Users.Add(engHead);
            ctx.Departments.Add(engd);

            ctx.Users.Add(storeClerk);
            ctx.Users.Add(storeSupervisor);
            ctx.Users.Add(storeManager);
            ctx.Departments.Add(stored);

            ctx.SaveChanges();

        }
    }
}