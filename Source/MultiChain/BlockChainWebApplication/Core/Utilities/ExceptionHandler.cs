using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace BlockChainWebApplication.Core.Utilities
{
    public class ExceptionHandler
    {
        public static void Handle(Exception ex)
        {
            try
            {
                string _body = JsonConvert.SerializeObject(ex);

                File.WriteAllText(HostingEnvironment.MapPath("/errors/" + Guid.NewGuid() + ".log"), _body);

                MailMessage _msg = new MailMessage();

                _msg.To.Add("admin@domain.com");
                _msg.From = new MailAddress("dev@domain.net", ".NET Development");
                _msg.Subject = "Application Error in Seratio Blockchain";
                _msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(_body, null, MediaTypeNames.Text.Html));

                SmtpClient _smtpClient = new SmtpClient("smtp.sendgrid.net", Convert.ToInt32(587));
                NetworkCredential _credentials = new NetworkCredential("ipvive", "1 sendgrid c#");
                _smtpClient.Credentials = _credentials;

                _smtpClient.Send(_msg);
            }
            catch
            {

            }
        }
    }
}