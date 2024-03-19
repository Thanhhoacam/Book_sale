using Microsoft.Extensions.Configuration;
using SendGrid.Helpers.Mail;
using SendGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace BulkyBook.Utility {
    public interface IEmailSender { Task SendEmailAsync(string email, string subject, string htmlMessage); }
    public class EmailSender : IEmailSender
    {
       

        public EmailSender() {
            
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage) {
            //logic to send email

           

            return Task.CompletedTask;


        }
    }
}

