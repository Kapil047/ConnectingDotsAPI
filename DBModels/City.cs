using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class City
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int CountryId { get; set; }

    public int StateId { get; set; }

    public bool Published { get; set; }

    public int DisplayOrder { get; set; }

    public virtual Country Country { get; set; } = null!;

    public virtual StateProvince State { get; set; } = null!;
}
