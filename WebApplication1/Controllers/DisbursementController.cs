using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.DataAccessLayer;
using System.Diagnostics;

namespace WebApplication1.Controllers
{
    public class DisbursementController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Disbursements(List<Item> items, List<int> reasons, List<int> inventory)
        {

            string[] keys = Request.Cookies.AllKeys;
            IDictionary<string, int> requestDetails = new Dictionary<string,int>();
            foreach(var i in keys)
            {
                try
                {
                    string value = Request.Cookies[i].Value;
                    int reqId = Convert.ToInt32(i);
                    int requestedAmount = Convert.ToInt32(value);
                    requestDetails.Add(i, requestedAmount);

                }
                catch(Exception e)
                {
                    continue;
                }
                
            }

            int count = 0;
            foreach (var i in items)
            {
                if (reasons[count] == 2)
                {
                    Debug.WriteLine("Raise adjustment: Item info: " + i.ItemId + " Shortfall: " + (inventory[count] - i.Quantity) + " Reason: " + reasons[count]);
                }
                count++;
            }
            return null;
        }

        
    }
}