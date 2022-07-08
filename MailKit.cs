using System;

using MailKit.Net.Smtp;
using MailKit;
using MimeKit;

namespace MailKit
{
    public class Email
    {
        public static void SendMessage(string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("New help request", "kirbybimhelpdesk@gmail.com"));
            message.To.Add(new MailboxAddress("Ignacio Benito Soto", "isoto@kirbygroup.com"));
            message.Subject = subject;

            message.Body = new TextPart("plain")
            {
                Text = body
            };

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, false);

                // Note: only needed if the SMTP server requires authentication
                client.Authenticate("kirbybimhelpdesk@gmail.com", "tuiy tawl agtd yype");

                client.Send(message);
                client.Disconnect(true);
            }
        }
    }
}
