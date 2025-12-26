using ConnectingDotsAPI.DBModels;

namespace ConnectingDotsAPI.Models
{
    public class OrderModelRequest
    {
        public class Order
        {
            public string? Id { get; set; }
            public string CustomOrderNumber { get; set; } = null!;

            public int? BillingAddressId { get; set; }

            public  required string CustomerId { get; set; }

            public int? PickupAddressId { get; set; }

            public int? ShippingAddressId { get; set; }

            public int OrderStatusId { get; set; }

            public string? CustomerCurrencyCode { get; set; }

            public string? VatNumber { get; set; }

            public string? CustomerIp { get; set; }
            public string? ShippingMethod { get; set; }
            public string? ShippingRateComputationMethodSystemName { get; set; }
            public string? CustomValuesXml { get; set; }
            public bool Deleted { get; set; }
            public DateTime? Podate { get; set; }
            public DateTime? Deadline { get; set; }
            public DateTime? DeliveryDate { get; set; }
            // Related Items and Notes
            public List<OrderItem> OrderItems { get; set; } = new();
            public List<OrderNote> OrderNotes { get; set; } = new();
        }
        public class OrderItem
        {
            public string? Id { get; set; }
            public int OrderId { get; set; }

            public required string ProductId { get; set; }

            public Guid? OrderItemGuid { get; set; }

            public int Quantity { get; set; }

            public string? AttributeDescription { get; set; }

            public string? Style { get; set; }

            public string? Color { get; set; }

            public string? Uom { get; set; }

            public string? Weight { get; set; }
        }
        public class OrderNote
        {
            public int? Id { get; set; }
            public string Note { get; set; } = null!;

            public int OrderId { get; set; }

            public int DownloadId { get; set; }

            public bool DisplayToCustomer { get; set; }
        }



    }

    public class OrderModel
    {
        public class OrderWithDetails
        {
            public Guid Id { get; set; }
            public string? CustomOrderNumber { get; set; }
            public int BillingAddressId { get; set; }
            public required string Customer{ get; set; }
            public Guid CustomerId{ get; set; }
            public required string OrderStatus { get; set; }
            public int OrderStatusId { get; set; }
            public DateTime CreatedOnUtc { get; set; }
            public DateTime? Podate { get; set; }
            public DateTime? Deadline { get; set; }
            public DateTime? DeliveryDate { get; set; }
            // Related Items and Notes
            public object OrderItems { get; set; } = new();
            public object OrderNotes { get; set; } = new();
        }

    }
}
