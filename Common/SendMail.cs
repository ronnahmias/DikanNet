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
        public static async Task SendEmailLink(IdentityMessage message) // send activation code
        {
            var fromEmail = new MailAddress("System.no-replay@dekan.co.il", "דיקאנט");
            var toEmail = new MailAddress(message.Destination);
            var fromEmailPassword = "R&r123456789";
           
            var smtp = new SmtpClient
            {
                Host = "dekan.co.il",
                Port = 25,
                UseDefaultCredentials = false,
                EnableSsl = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword,"dekan.co.il")
            };
           
            using (var message1 = new MailMessage(fromEmail, toEmail)
            {
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = true
            })

            smtp.Send(message1);
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
            var from = new EmailAddress("System.no-replay@dekan.co.il", "דיקאנט");
            var to = new EmailAddress(message.Destination);
            var msg = MailHelper.CreateSingleEmail(from, to, message.Subject, null, message.Body);
            var response = await client.SendEmailAsync(msg);
        }
    }
}
