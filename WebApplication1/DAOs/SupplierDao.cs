using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.DataAccessLayer;
using WebApplication1.Models;

namespace WebApplication1.DAOs
{
    public class SupplierDao
    {
        public static List<Supplier> GetSuppliers()
        {
            using (var ctx = new UniDBContext())
            {
                return ctx.Suppliers.OrderBy(s => s.Priority).ToList();
            }
        }
    }
}