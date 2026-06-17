using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class Country
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? TwoLetterIsoCode { get; set; }

    public string? ThreeLetterIsoCode { get; set; }

    public bool AllowsBilling { get; set; }

    public bool AllowsShipping { get; set; }

    public int NumericIsoCode { get; set; }

    public bool SubjectToVat { get; set; }

    public bool Published { get; set; }

    public int DisplayOrder { get; set; }

    public bool LimitedToStores { get; set; }

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

    public virtual ICollection<City> Cities { get; set; } = new List<City>();

    public virtual ICollection<Property> Properties { get; set; } = new List<Property>();

    public virtual ICollection<StateProvince> StateProvinces { get; set; } = new List<StateProvince>();
}
