using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using WebApplication1.Models;

namespace WebApplication1.DataAccessLayer
{
    public class UniDBContext : DbContext
    {
        public UniDBContext() : base("name=LogicUniversityConString")
        {
            Database.SetInitializer(new DBInitializer());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasRequired<Department>(s => s.Department)
                .WithMany(d => d.Users);
            
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Item> Items { get; set; }

        public DbSet<Department> Departments { get; set; }

        public DbSet<Supplier> Suppliers { get; set; }

        public DbSet<Request> Requests { get; set; }
        public DbSet<RequestDetail> RequestDetails { get; set; }

        public DbSet<Disbursement> Disbursements { get; set; }
        public DbSet<DisbursementDetail> DisbursementDetails { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        public DbSet<Adjustment> Adjustments { get; set; }
        public DbSet<AdjustmentDetail> AdjustmentDetails { get; set; }
    }
}