using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class Traceability
{
    public int Id { get; set; }

    public Guid Guid { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int? ParentNodeId { get; set; }

    public int TypeId { get; set; }

    public int? SupplierId { get; set; }

    public int? ProductId { get; set; }

    public int ManagerUserId { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public bool Deleted { get; set; }

    public virtual ICollection<Traceability> InverseParentNode { get; set; } = new List<Traceability>();

    public virtual User ManagerUser { get; set; } = null!;

    public virtual Traceability? ParentNode { get; set; }

    public virtual Product? Product { get; set; }

    public virtual User? Supplier { get; set; }

    public virtual ReferenceCode Type { get; set; } = null!;
}
