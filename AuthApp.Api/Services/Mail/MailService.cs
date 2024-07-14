using AuthApp.Api.Configurations;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace AuthApp.Api.Services.Mail
{
    public class MailService : IMailService
    {
        private readonly EmailSettings _emailSettings;

        public MailService(IOptions<EmailSettings> emailOptions)
        {
            _emailSettings = emailOptions.Value;
        }

        public bool Send(string sender, string subject, string body, bool isBodyHTML)
        {
            try
            {
                MailMessage mailMessage = new MailMessage();    

                mailMessage.From = new MailAddress(_emailSettings.Email);
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = isBodyHTML;

                mailMessage.To.Add(new MailAddress(sender));  

                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";

                smtp.EnableSsl = true;

                NetworkCredential networkCredential = new NetworkCredential();
                networkCredential.UserName = mailMessage.From.Address;
                networkCredential.Password = _emailSettings.Password;

                // If using gmail, set it true
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = networkCredential;

                // If using google set it 587
                smtp.Port = 587;

                // Send 
                smtp.Send(mailMessage);
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
