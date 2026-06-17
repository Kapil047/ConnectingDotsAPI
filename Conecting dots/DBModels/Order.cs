using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class Order
{
    public int Id { get; set; }

    public string CustomOrderNumber { get; set; } = null!;

    public int? BillingAddressId { get; set; }

    public int CustomerId { get; set; }

    public int? PickupAddressId { get; set; }

    public int? ShippingAddressId { get; set; }

    public Guid OrderGuid { get; set; }

    public int OrderStatusId { get; set; }

    public string? CustomerCurrencyCode { get; set; }

    public string? VatNumber { get; set; }

    public string? CustomerIp { get; set; }

    public string? ShippingMethod { get; set; }

    public string? ShippingRateComputationMethodSystemName { get; set; }

    public string? CustomValuesXml { get; set; }

    public bool Deleted { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? Podate { get; set; }

    public DateTime? Deadline { get; set; }

    public DateTime? DeliveryDate { get; set; }

    public virtual Address? BillingAddress { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<OrderNote> OrderNotes { get; set; } = new List<OrderNote>();

    public virtual Address? PickupAddress { get; set; }

    public virtual Address? ShippingAddress { get; set; }
}
