using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class ReferenceCode
{
    public int Id { get; set; }

    public int ReferenceTypeId { get; set; }

    public string SystemKeyword { get; set; } = null!;

    public string Name { get; set; } = null!;

    public bool Enabled { get; set; }

    public int? ReferenceCodeId { get; set; }

    public int SortOrder { get; set; }

    public virtual ICollection<ReferenceCode> InverseReferenceCodeNavigation { get; set; } = new List<ReferenceCode>();

    public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();

    public virtual ReferenceCode? ReferenceCodeNavigation { get; set; }

    public virtual ReferenceType ReferenceType { get; set; } = null!;

    public virtual ICollection<Traceability> Traceabilities { get; set; } = new List<Traceability>();
}
