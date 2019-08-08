using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public enum UserRank
    {
        Head,
        Employee,
        Supervisor,
        Manager,
        Clerk,
        TemporaryHead
    }

    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        [Required(ErrorMessage ="Please Enter Username",AllowEmptyStrings = false)]
        public string Username { get; set; }
        [Required(ErrorMessage = "Please Enter Password", AllowEmptyStrings = false)]
        public string Password { get; set; }
        public int Rank { get; set; }
        public Department Department { get; set; }
    }
}