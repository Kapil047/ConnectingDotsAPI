using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class Customer
{
    public int Id { get; set; }

    public Guid Guid { get; set; }

    public string? Username { get; set; }

    public string? Email { get; set; }

    public string? EmailToRevalidate { get; set; }

    public string? SystemName { get; set; }

    public int? BillingAddressId { get; set; }

    public int? ShippingAddressId { get; set; }

    public string? AdminComment { get; set; }

    public bool IsTaxExempt { get; set; }

    public int AffiliateId { get; set; }

    public bool RequireReLogin { get; set; }

    public int FailedLoginAttempts { get; set; }

    public DateTime? CannotLoginUntilDateUtc { get; set; }

    public bool Active { get; set; }

    public bool Deleted { get; set; }

    public bool IsSystemAccount { get; set; }

    public string? LastIpAddress { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? LastLoginDateUtc { get; set; }

    public DateTime LastActivityDateUtc { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public virtual Address? BillingAddress { get; set; }

    public virtual ICollection<CustomerAuthToken> CustomerAuthTokens { get; set; } = new List<CustomerAuthToken>();

    public virtual ICollection<CustomerPassword> CustomerPasswords { get; set; } = new List<CustomerPassword>();

    public virtual ICollection<FormResponse> FormResponses { get; set; } = new List<FormResponse>();

    public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();

    public virtual ICollection<Log> Logs { get; set; } = new List<Log>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual Address? ShippingAddress { get; set; }
}
