using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mime;
using System.Web.Mvc;
using System.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.AspNet.Identity;

namespace Common
{
    public static class SendMail
    {
        public static void SendEmailLink(string UserEmail, string body, string subject) // send activation code
        {
            var fromEmail = new MailAddress("DikannetProject@gmail.com", "דיקאנט");
            var toEmail = new MailAddress(UserEmail);
            var fromEmailPassword = "r&r123456";
           
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };
           
            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })

            smtp.Send(message);
        }

        public static string CreateBodyEmail(string name, string link, string message) // create email body templete
        {
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            string body = string.Empty;
            using (StreamReader read = new StreamReader(HttpContext.Current.Server.MapPath("~/Content/EmailTmp/EmailTemplete.html")))
            {
                body = read.ReadToEnd();
            }
            body = body.Replace("{name}", name);
            body = body.Replace("{link}",link);
            body = body.Replace("{message}", message);
            body = body.Replace("{homepage}", url.Action("Login", "Login", null));
            return body;
        }

        public static async Task configSendGridasync(IdentityMessage message)
        {
            var apikey = ConfigurationManager.AppSettings["SendGridAPIKey"]; // token of send grid from web.config
            var client = new SendGridClient(apikey);
            var from = new EmailAddress("dikannetproject@gmail.com", "דיקאנט");
            var to = new EmailAddress(message.Destination);
            var msg = MailHelper.CreateSingleEmail(from, to, message.Subject, null, message.Body);
            var response = await client.SendEmailAsync(msg);
        }
    }
}
