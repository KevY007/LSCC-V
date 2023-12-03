using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Mail;
using GTANetworkAPI;

namespace RNC
{
    public class EmailController
    {
        public static readonly string SMTP_HOST = "mail.smtp2go.com";
        public static readonly int SMTP_PORT = 8025;
        public static readonly string SMTP_USER = "uB4QVa3fk5DeHNC";
        public static readonly string SMTP_PASS = "KavTUNuDbxgF4wmHRmpGVn8MNmftpuE4";

        public static readonly string EMAIL_FROM = "noreply@racenchase.com";

        public static void SendEmail(string recipient, string subject, string body)
        {
            MailMessage message = new MailMessage(EMAIL_FROM, recipient);
            message.Subject = subject;
            message.Body = body;

            SmtpClient smtpClient = new SmtpClient(SMTP_HOST, SMTP_PORT);
            smtpClient.Credentials = new NetworkCredential(SMTP_USER, SMTP_PASS);
            smtpClient.EnableSsl = true;
            try
            {
                smtpClient.Send(message);
                NAPI.Util.ConsoleOutput($"[E-MAILER] Successfully e-mailed {recipient}.");
            }
            catch(Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[E-MAILER] Failed to send email to {recipient}: {ex.ToString()}");
            }
        }

    }
}
