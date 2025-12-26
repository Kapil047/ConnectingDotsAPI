using System.Net;
using ETGenericApp.Models;
using ConnectingDotsAPI.Services.SettingsService;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace ConnectingDotsAPI.Services.MailService
{
    public class MailService : IMailService
    {
        private IConfiguration Config { get; }
        private readonly ISettingsService settingsService;
        private string sendgridapikey = string.Empty;
        public MailService(IConfiguration config, ISettingsService settingsService)
        {

            Config = config;
            this.settingsService = settingsService;
            sendgridapikey= settingsService.FindConfiguration("sendgridapikey")??throw new Exception("API KEY MISSING");
        }

        public async Task<HttpStatusCode> SendMail(MailModel.SendMailDetails mail)
        {
            MailModel.SendGridMailMessageRequest mailmessage = new()
            {
                Body = mail.Body,
                Attachments = mail.Attachments ?? [],
                BCC = mail.BCC ?? [],
                CC = [mail.CC],
                Subject = mail.Subject,
                ToName = mail.To,
                ToAddress = [mail.To]
            };

            switch (mail.Client.Trim().ToLower())
            {
                case "test":
                    {
                        mailmessage.FromAddress = null;
                        mailmessage.ToAddress = ["ensuenotechnologies@gmail.com"];
                        break;
                    }
                case "vastu-properties":
                    {
                        // mailmessage.FromAddress = "noreply@bridgingimmigrations.com";
                        mailmessage.ToAddress = ["info@vastu-propertie.com"];
                        break;
                    }
                case "banker-buddy":
                    {
                        // mailmessage.FromAddress = "noreply@bridgingimmigrations.com";
                        mailmessage.ToAddress = ["info@bankerbuddy.co.in"];
                        break;
                    }

                default:
                    {
                        mailmessage.FromAddress = null;
                        // mailmessage.ToAddress = null;
                        break;
                    }
            }

            var response = await SendViaSendGridAsync(mailmessage);

            return response;
        }
        private async Task<HttpStatusCode> SendViaSendGridAsync(MailModel.SendGridMailMessageRequest request)
        {
            try
            {
                //pulling from appsettings.json

                var client = new SendGridClient(sendgridapikey);

                var subject = request.Subject;
                var from = string.IsNullOrEmpty(request.FromAddress) ? "mail@ensuenotech.com" : request.FromAddress;
                var tos = new List<EmailAddress>();
                var ccs = new List<EmailAddress>();
                var bccs = new List<EmailAddress>();
                if (request.ToAddress.Count == 0) throw new Exception("NO EMAIL ADDRESS TO SEND");
                request.ToAddress.ForEach(t =>
                {
                    tos.Add(new EmailAddress(t, t));
                });
                request.CC?.ForEach(_cc =>
                {
                    if (_cc != null)
                        if (!request.ToAddress.Any(em => em == _cc))
                            ccs.Add(new EmailAddress(_cc, _cc));
                });
                request.BCC?.ForEach(_cc =>
                {
                    if (_cc != null)
                        if (!(request.ToAddress.Any(em => em == _cc) || request.CC.Any(em => em == _cc)))
                            bccs.Add(new EmailAddress(_cc, _cc));
                });


                var _attachments = new List<Attachment>();
                if (request.Attachments != null)
                {
                    foreach (var url in request.Attachments)
                    {
                        using HttpClient httpClient = new();
                        var _response = await httpClient.GetAsync(url);
                        // Get the file extension from the URL
                        string fileExtension = Path.GetExtension(url);

                        // Remove the leading dot from the extension
                        if (!string.IsNullOrEmpty(fileExtension))
                        {
                            fileExtension = fileExtension.TrimStart('.');
                        }

                        if (_response.IsSuccessStatusCode)
                        {
                            byte[] responseData = await _response.Content.ReadAsByteArrayAsync();
                            _attachments.Add(new Attachment
                            {
                                Content = Convert.ToBase64String(responseData),
                                Filename = $"{Guid.NewGuid()} | {Path.GetFileName(url)}",
                                Type = _response.Content.Headers.ContentType.MediaType,
                                Disposition = "attachment"
                            });
                        }
                        throw new Exception($"{await _response.Content.ReadAsStringAsync()}_{_response.ReasonPhrase}");
                    };
                }

                var message = new SendGridMessage
                {
                    Personalizations =
                [
                    new()
                    {
                        Tos = tos,
                    }

                ]
                };
                if (ccs.Count > 0)
                {
                    message.Personalizations.FirstOrDefault().Ccs = ccs;
                }
                if (bccs.Count > 0)
                {
                    message.Personalizations.FirstOrDefault().Bccs = bccs;
                }
                message.From = new(from, from);
                message.ReplyTo = new(from, from);
                message.Subject = subject;
                request.FromName = "eET";

                message.Contents =
                [
                    new Content()
                    {
                        Type="text/html",
                        Value = request.Body
                    }
                ];
                if (_attachments.Count > 0)
                    message.Attachments = _attachments;
                var response = await client.SendEmailAsync(message).ConfigureAwait(false);
                return response.StatusCode;
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }
    }
}