using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Mail;
using GTANetworkAPI;
[assembly: MethodCallLogger(AttributeTargetTypes = "RNC.*", AttributeTargetMembers = "*")]

namespace RNC
{
    public class EmailController
    {

        public static void SendEmail(string recipient, string subject, string body, bool isHTML = false)
        {
            MailMessage message = new MailMessage(Settings.Dynamics.SMTP_EMAIL_FROM, recipient);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = isHTML;

            SmtpClient smtpClient = new SmtpClient(Settings.Dynamics.SMTP_HOST, Settings.Dynamics.SMTP_PORT);
            smtpClient.Credentials = new NetworkCredential(Settings.Dynamics.SMTP_USER, Settings.Dynamics.SMTP_PASS);
            smtpClient.EnableSsl = true;
            try
            {
                smtpClient.Send(message);
                NAPI.Util.ConsoleOutputEx($"[E-MAILER] Successfully e-mailed {recipient}.");
            }
            catch(Exception ex)
            {
                NAPI.Util.ConsoleOutputEx($"[E-MAILER] Failed to send email to {recipient}: {ex.ToString()}");
            }
        }

    }
}
