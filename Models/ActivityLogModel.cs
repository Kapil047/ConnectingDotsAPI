namespace ConnectingDotsAPI.Models
{
    public class ActivityLogModel
    {
        public class LogActivityRequest
        {
            public string Comment { get; set; } = null!;

            public string? IpAddress { get; set; }

            public string? EntityName { get; set; }

            public int ActivityLogTypeId { get; set; }

            public int? UserId { get; set; }

            public int? EntityId { get; set; }
        }
        
    }
}
