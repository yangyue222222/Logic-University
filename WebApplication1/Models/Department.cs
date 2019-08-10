using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }
        public User Representative { get; set; }
        public string DepartmentName { get; set; }
        public ICollection<User> Users { get; set; }
        public PickUpPoint PickupPoint { get; set; }
    }
}