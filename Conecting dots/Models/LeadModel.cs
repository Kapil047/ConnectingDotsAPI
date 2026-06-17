using ConnectingDotsAPI.DBModels;
using static ConnectingDotsAPI.Models.PropertyModel;

namespace ConnectingDotsAPI.Models
{
    public class LeadModel
    {
        public class LeadRequest
        {
            public string? Guid { get; set; }
            public string? LeadName { get; set; }
            public string? Email { get; set; }
            public string? PhoneNumber { get; set; }
            public string? PropertyId { get; set; }
            public string? ProductId { get; set; }
            public string? CustomerId { get; set; }
            public int? Source { get; set; }
            public int? Status { get; set; }
            public string? AssignedTo { get; set; }
            public Dictionary<string, string>? Attributes { get; set; }
            public string? Remarks { get; set; }
        }

        public class LeadDetails
        {
            public Guid Guid { get; set; }
            public string? LeadName { get; set; }
            public string? Email { get; set; }
            public string? PhoneNumber { get; set; }
            public PropertyDetails? PropertyDetails { get; set; }
            public ProductModel.ProductDetails? ProductDetails { get; set; }
            public CustomerModel.CustomerDetails? Customer { get; set; }
            public object? Source { get; set; }
            public object? AvailableStatus { get; set; }
            public required object Status { get; set; }
            public object? AssignedTo { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public object? Attributes { get; set; }
            public object? FollowUp { get; set; }
            public string? Remarks { get; set; }
        }

        public class LeadStatusDetails
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
            public bool Active { get; set; }
            public int? PreviousStatusId { get; set; }
            public bool InputRequried { get; set; }
            public string? InputControlType { get; set; }
            public string? InputNotes { get; set; }
            public object? SubStatus { get; set; }
            public object? NextStatus { get; set; }
            public object? ParentStatus { get; set; }
        }
        public class LeadStatusRequest
        {
            public int? Id { get; set; }

            public string Name { get; set; } = null!;

            public string? Description { get; set; }

            public bool Active { get; set; }

            public int? ParentId { get; set; }

            public int? PreviousStatusId { get; set; }

            public bool InputRequried { get; set; }

            public string? InputControlType { get; set; }

            public string? InputNotes { get; set; }
        }

        public class FollowUpRequest
        {
            public int? Id { get; set; }
            public required string LeadId { get; set; }
            public required DateTime FollowUpDate { get; set; }
            public string? Notes { get; set; } 
        }


    }
}
