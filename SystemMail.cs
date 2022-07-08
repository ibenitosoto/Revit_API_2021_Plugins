using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace API_2021_Plugins
{
    public class SystemMail
    {
        public static void CreateTestMessage(string guid, string subject, string body)
        {
            var fromAddress = new MailAddress("kirbybimhelpdesk@gmail.com", "New BIM help request received");
            var fromPassword = "tuiy tawl agtd yype";
            var toAddress = new MailAddress("isoto@kirbygroup.com");

        
            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential(fromAddress.Address, fromPassword)
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = $"BIM Request ID: {guid} - Subject: {subject}",
                Body = body
            })

                smtp.Send(message);
        }
    }
}
