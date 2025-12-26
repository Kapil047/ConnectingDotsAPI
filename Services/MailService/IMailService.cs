using System.Net;
using ETGenericApp.Models;

namespace ConnectingDotsAPI.Services.MailService
{
    public interface IMailService
    {
        Task<HttpStatusCode> SendMail(MailModel.SendMailDetails mail);
    }
}