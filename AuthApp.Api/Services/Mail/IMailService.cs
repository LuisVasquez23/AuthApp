namespace AuthApp.Api.Services.Mail
{
    public interface IMailService
    {
        bool Send(string sender, string subject, string body, bool isBodyHTML);
    }
}
