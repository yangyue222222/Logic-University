using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Filters;
using WebApplication1.Models;
using WebApplication1.DAOs;

namespace WebApplication1.Controllers
{
    [AuthFilter]
    public class TrendController : Controller
    {
        // GET: Trend
        [HttpGet, Route("TrendAnalysis", Name = "TrendAnalysis")]
        [AuthorizeFilter((int)UserRank.Manager,(int)UserRank.Supervisor,(int)UserRank.Clerk)]
        public ActionResult GetTrendPage()
        {
            List<string> categories = ItemDao.GetCategoryNames();
            ViewData["Categories"] = categories;

            return View("Chart");
        }

        [HttpGet, Route("getstockdata")]
        [AuthorizeFilter((int)UserRank.Manager, (int)UserRank.Supervisor, (int)UserRank.Clerk)]
        public ActionResult GetDataByCategory(string itemCategory)
        {
            Dictionary<string, Dictionary<int, int>> itemInfo = RequestDao.GetRequestedItemNoByCategory(itemCategory);
            Dictionary<string, List<object>> result = new Dictionary<string, List<object>>();
            foreach(KeyValuePair<string,Dictionary<int,int>> kV in itemInfo)
            {
                List<object> itemMonthQtyList = new List<object>();
                foreach(KeyValuePair<int,int> kVNested in kV.Value)
                {
                    var obj = new
                    {
                        month = kVNested.Key,
                        qty = kVNested.Value
                    };

                    itemMonthQtyList.Add(obj);
                }

                result.Add(kV.Key,itemMonthQtyList);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}