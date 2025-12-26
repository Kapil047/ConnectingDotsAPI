using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class Form
{
    public int Id { get; set; }

    public Guid Guid { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool Active { get; set; }

    public bool Deleted { get; set; }

    public int DisplayOrder { get; set; }

    public virtual ICollection<FormResponse> FormResponses { get; set; } = new List<FormResponse>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<Product> ProductsNavigation { get; set; } = new List<Product>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
