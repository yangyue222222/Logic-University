using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.DataAccessLayer;
using WebApplication1.Models;

namespace WebApplication1.DAOs
{
    public class ItemDao
    {
        public Dictionary<string,List<Item>> getItemsForRequisition()
        {
            Dictionary<string, List<Item>> ItemsList = new Dictionary<string,List<Item>>();
            using (var ctx = new UniDBContext())
            {
                ItemsList = ctx.Items.GroupBy(i => i.Category).ToDictionary(i => i.Key, i => i.ToList());
            }

            return ItemsList;
        }
    }
}