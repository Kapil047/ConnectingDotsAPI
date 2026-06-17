namespace ConnectingDotsAPI.Models
{
    public class TraceabilityModel
    {
        public class TraceabilityRequest
        {
            public int? Id { get; set; }
            public string? Guid { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string? ParentNodeId { get; set; }
            public int TypeId { get; set; }
            public string? SupplierId { get; set; }
            public string? ProductId { get; set; }
        }
        public class TraceabilityDetails
        {
            public required Guid Guid { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public object? ParentNode { get; set; }
            public required object Type { get; set; }
            public object? Supplier { get; set; }
            public object? Product { get; set; }
            public required object ManagerUser { get; set; }
        }

    }
}
