
namespace ETGenericApp.Models
{
    public class MailModel
    {
        public class SendMailDetails
        {
            public required string Body { get; set; }
            public required string Subject { get; set; }
            public required string To { get; set; }
            public string? CC { get; set; }
            public List<string>? BCC { get; set; }
            public string? ReplyTo { get; set; }
            public List<string>? Attachments { get; set; }
            public required string Client { get; set; }
        }
        #region SendGrid
        public class SendGridMailMessageRequest
        {
            public string? FromAddress { get; set; }
            public string? FromName { get; set; }
            public List<string> ToAddress { get; set; } = [];
            public required string ToName { get; set; }
            public required string Subject { get; set; }
            public List<string> CC { get; set; } = [];
            public List<string> BCC { get; set; } = [];
            public required string Body { get; set; }
            public List<string> Attachments { get; set; }=[];
        }
        public class FileDetails
        {
            public int? Id { get; set; }
            public required string FileName { get; set; }
            public required string Url { get; set; }
            public required string Blob { get; set; }
        }
        #endregion
    }
}

