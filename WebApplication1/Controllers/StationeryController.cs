using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.DAOs;
using WebApplication1.Models;
using System.Diagnostics;
using WebApplication1.Filters;

namespace WebApplication1.Controllers
{
    [AuthFilter]
    public class StationeryController : Controller
    {
        [HttpGet]
        [AuthorizeFilter((int)UserRank.Clerk)]
        public ActionResult Retrieval()
        {
            List<RetrievalItem> items = DisbursementDao.GetAllItemsForRetrieval();

            ViewData["RetrievalItems"] = items;
            return View();
        }

        [HttpGet]
        public ActionResult RetrievalMobile()
        {
            List<RetrievalItem> items = DisbursementDao.GetAllItemsForRetrieval();
            return Json(items, JsonRequestBehavior.AllowGet);
        }


        //[HttpGet]
        //public ActionResult Retrieval()
        //{
        //    List<Item> inventory = new List<Item>();
        //    List<RetrievalItem> items = RequestDao.GetRequestDeailsByItems();
        //    foreach(var i in items)
        //    {
        //        HttpCookie cookie = new HttpCookie(i.ItemId.ToString());
        //        cookie.Value = i.Quantity.ToString();
        //        cookie.Expires = DateTime.Now.AddHours(3);
        //        Response.Cookies.Add(cookie);
        //        Item add = ItemDao.getItemById(i.ItemId);
        //        inventory.Add(add);
        //    }
        //    ViewData["Inventory"] = inventory;
        //    ViewData["RetrievalItems"] = items;
        //    return View();
        //}


        //public JsonResult GetRequestDetails(int sortBy)
        //{
        //    Debug.WriteLine("Request Details");
        //    Debug.WriteLine(sortBy);
        //    if (sortBy == 0)
        //    {
        //        List<RetrievalItem> items = RequestDao.GetRequestDeailsByItems();
        //        Debug.WriteLine(items);
        //        foreach(var i in items)
        //        {
        //            Debug.WriteLine(i.Description);
        //        }
        //        return Json(items,JsonRequestBehavior.AllowGet);
        //    }else if (sortBy == 1)
        //    {
        //        List<Request> requests = RequestDao.GetRequestDetailByDepartment();
        //        List<object> listObjects = new List<object>();
        //        foreach(var r in requests)
        //        {
        //            List<object> obj = new List<object>();
        //            foreach(var detail in r.RequestDetails)
        //            {
        //                obj.Add(new
        //                {
        //                    name = detail.Item.Description,
        //                    count = detail.Quantity
        //                });

        //            }
        //            listObjects.Add(
        //                new
        //                {
        //                    departmentname = r.Department.DepartmentName,
        //                    items = obj
        //                }
        //            );
        //        }
        //        return Json(listObjects,JsonRequestBehavior.AllowGet);
        //    }

        //    return null;

        //}
    }
}
