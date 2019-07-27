using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.Models;
using WebApplication1.DataAccessLayer;
using System.Diagnostics;

namespace WebApplication1.DAOs
{
    public class RequestDao
    {
        public static void InsertRequest(ICollection<Item> items, User u)
        {
            try
            {
                using (var ctx = new UniDBContext())
                {
                    Request req = new Request();
                    req.Requestor = u;
                    req.Department = u.Department;
                    ICollection<RequestDetail> details = new List<RequestDetail>();
                    req.Date = DateTime.UtcNow;
                    req.Status = (int)RequestStatus.Requested;
                    foreach (var i in items)
                    {
                        RequestDetail detail = new RequestDetail();
                        ctx.Items.Attach(i);
                        detail.Item = i;
                        detail.Quantity = i.Quantity;
                        details.Add(detail);
                    }

                    ctx.Users.Attach(u);
                    ctx.Departments.Attach(req.Department);
                    req.RequestDetails = details;
                    ctx.RequestDetails.AddRange(details);
                    ctx.Requests.Add(req);
                    ctx.SaveChanges();
                }
            }catch (Exception e)
            {
                throw new Exception("Internal Server Error");
            }
        }

        public static List<Request> getRequestsByDepartment(int departmentId)
        {
            try
            {
                using(var ctx = new UniDBContext())
                {
                    List<Request> requests = ctx.Requests.Include("Requestor").Where(r => r.Requestor.Department.DepartmentId == departmentId && r.Status == (int)RequestStatus.Requested).ToList();
                    
                    return requests;
                }
            }catch(Exception e)
            {
                return null;
            }
        }

        public static List<RequestDetail> getRequestDetailsByRequestId(int id,int departmentId)
        {
            List<RequestDetail> details = null;
            using(var ctx = new UniDBContext())
            {
                details = ctx.RequestDetails.Include("Item").Where(d => d.Request.RequestId == id && d.Request.Department.DepartmentId == departmentId).ToList();
            }
            return details;
        }

        public static void updateRequestStatus(Request request)
        {
            using(var ctx = new UniDBContext())
            {
                var req = ctx.Requests.Where(r => r.RequestId == request.RequestId && r.Department.DepartmentId == request.Department.DepartmentId).SingleOrDefault();
                req.Status = request.Status;
                req.ApprovedBy = request.ApprovedBy;
                ctx.Users.Attach(req.ApprovedBy);
                ctx.SaveChanges();
            }
        }
        
    }
}