using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class ReferenceType
{
    public int Id { get; set; }

    public string SystemKeyword { get; set; } = null!;

    public string Name { get; set; } = null!;

    public bool Enabled { get; set; }

    public virtual ICollection<ReferenceCode> ReferenceCodes { get; set; } = new List<ReferenceCode>();
}
