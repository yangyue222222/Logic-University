using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using WebApplication1.DAOs;
using WebApplication1.Models;
namespace WebApplication1.Utilities
{
    public class EmailUtility
    {
        public async static Task SendEmailForApproval(int departmentId)
        {
            var senderEmail = new MailAddress("sa48t4ad@gmail.com");
            Task<User> deptHeadTask = UserDao.GetDepartmentHeadByDepartmentId(departmentId);
            User deptHead = await deptHeadTask;

            var receiverEmail = new MailAddress(deptHead.Email);


            var smtp = new SmtpClient()
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(senderEmail.Address, "spottybottyhotty")
            };

            var msg = new MailMessage(senderEmail, receiverEmail)
            {
                Subject = "Requisitions Approval",
                Body = "You have requisitions awaiting for approval. Please sign in to the system to review them."
            };

            smtp.Send(msg);

        }


        public async static Task SendEmailForItemsPickUp()
        {
            var senderEmail = new MailAddress("sa48t4ad@gmail.com");
            List<int> deptIds = await DisbursementDao.GetDepartmentIdWithPreparedDisbursement();
            List<User> reps = UserDao.FindRepresentativeByDepartmentIds(deptIds);

            


            var smtp = new SmtpClient()
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(senderEmail.Address, "spottybottyhotty")
            };

            foreach(var u in reps)
            {
                if(u.Email != null || u.Email != "")
                {
                    var receiverEmail = new MailAddress(u.Email);
                    var msg = new MailMessage(senderEmail, receiverEmail)
                    {
                        Subject = "Items Pick Up",
                        Body = "The store side has prepared for disbursements. Please check it on items pick up tab."
                    };

                    smtp.Send(msg);
                }
            }

            

        }
    }
}