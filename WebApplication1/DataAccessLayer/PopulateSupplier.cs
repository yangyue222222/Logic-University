using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.Models;

namespace WebApplication1.DataAccessLayer
{
    public class PopulateSupplier
    {
        public void populateSuppliers(UniDBContext ctx)
        {
            List<Supplier> suppliers = new List<Supplier>();
            Supplier s1 = new Supplier()
            {
                SupplierCode = "ALPA",
                Address = "Blk 1128, Ang Mo Kio Industrial Park #02-1108 aNG mO kIO sTREET 62 sINGAPORE 622262",
                Priority = 1,
                ContactName = "Ms Irene Tan",
                PhoneNo = "4619928",
                FaxNo = "4612238",
                SupplierName = "ALPHA Office Supplies"
            };

            Supplier s2 = new Supplier()
            {
                SupplierCode = "CHEP",
                Address = "Blk 34, Clementi Road #07-02 Ban Ban Soh Building Singapore 110525",
                Priority = 2,
                ContactName = "Mr Soh Kway Koh",
                PhoneNo = "3543234",
                FaxNo = "4742434",
                SupplierName = "Cheap Stationer"
            };

            Supplier s3 = new Supplier()
            {
                SupplierCode = "BANE",
                Address = "Blk 124, Alexandra Road #03-04 Banes Building Singapore 550315",
                Priority = 3,
                ContactName = "Mr Loh Ah Pek",
                PhoneNo = "4781234",
                FaxNo = "4792434",
                SupplierName = "BANES Shop"
            };

            Supplier s4 = new Supplier()
            {
                SupplierCode = "OMEG",
                Address = "Blk 11, Hillview Avenue #03-04, Singapore 679036",
                Priority = 4,
                ContactName = "Mr Ronnie ho",
                PhoneNo = "7671233",
                FaxNo = "7671234",
                SupplierName = "OMEGA Stationery Supplier"
            };

            suppliers.Add(s1);
            suppliers.Add(s2);
            suppliers.Add(s3);
            suppliers.Add(s4);

            ctx.Suppliers.AddRange(suppliers);
        } 
    }
}