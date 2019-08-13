using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.Models;
using WebApplication1.DataAccessLayer;
using System.Threading.Tasks;

namespace WebApplication1.DAOs
{
    public class PickUpPointDao
    {
        public async static Task<List<PickUpPoint>> GetAllPickupPoints()
        {
            using(var ctx = new UniDBContext())
            {
                List<PickUpPoint> points = ctx.PickUpPoints.ToList();
                return points;
            }
        }

        public async static Task<PickUpPoint> GetPickupPointByDepartment(int departmentId)
        {
            using (var ctx = new UniDBContext())
            {
                PickUpPoint point = ctx.Departments.Include("PickupPoint").Where(d => d.DepartmentId == departmentId).Select(d => d.PickupPoint).SingleOrDefault();

                return point;
            }
        }
    }
}