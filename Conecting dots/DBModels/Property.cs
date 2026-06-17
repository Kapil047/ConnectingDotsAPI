using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class Property
{
    public int Id { get; set; }

    public Guid Guid { get; set; }

    public string PropertyName { get; set; } = null!;

    public string PropertyType { get; set; } = null!;

    public string AddressLine1 { get; set; } = null!;

    public string? AddressLine2 { get; set; }

    public string City { get; set; } = null!;

    public int State { get; set; }

    public string PostalCode { get; set; } = null!;

    public int Country { get; set; }

    public decimal Price { get; set; }

    public int Bedrooms { get; set; }

    public int Bathrooms { get; set; }

    public int SquareFeet { get; set; }

    public string? Description { get; set; }

    public DateTime ListingDate { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool Deleted { get; set; }

    public virtual Country CountryNavigation { get; set; } = null!;

    public virtual StateProvince StateNavigation { get; set; } = null!;
}
