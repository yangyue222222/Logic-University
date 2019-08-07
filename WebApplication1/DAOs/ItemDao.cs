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
        public static Dictionary<string,List<Item>> getItemsForRequisition()
        {
            Dictionary<string, List<Item>> ItemsList = new Dictionary<string,List<Item>>();
            using (var ctx = new UniDBContext())
            {
                ItemsList = ctx.Items.GroupBy(i => i.Category).ToDictionary(i => i.Key, i => i.ToList());
            }

            return ItemsList;
        }
        public static Item getItemById(int itemId)
        {
            Item item = null;
            using (var ctx = new UniDBContext())
            {
                item = ctx.Items.Where(i => i.ItemId == itemId).FirstOrDefault();
            }
            return item;
        }

        public static void UpdateStockForAdjustment(List<AdjustmentDetail> details) 
        {
            using(var ctx = new UniDBContext())
            {
                List<int> itemIds = details.Select(de => de.Item.ItemId).ToList();
                Dictionary<int, Item> itemDict = ctx.Items.Where(i => itemIds.Contains(i.ItemId)).ToDictionary(i => i.ItemId);
                foreach (var d in details)
                {
                    Item item = itemDict[d.Item.ItemId];
                    item.Quantity = (item.Quantity - d.Count); 

                }

                ctx.SaveChanges();
            }
        }

        public static List<Item> GetAllItems()
        {
            using(var ctx = new UniDBContext())
            {
                List<Item> items = ctx.Items.ToList();
                return items;
            }
        }

    }
}