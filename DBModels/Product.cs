using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class Product
{
    public int Id { get; set; }

    public Guid Guid { get; set; }

    public string Name { get; set; } = null!;

    public string? MetaKeywords { get; set; }

    public string? MetaTitle { get; set; }

    public string? Sku { get; set; }

    public string? ShortDescription { get; set; }

    public string? FullDescription { get; set; }

    public string? AdminComment { get; set; }

    public bool ShowOnHomepage { get; set; }

    public string? MetaDescription { get; set; }

    public bool HasUserAgreement { get; set; }

    public string? UserAgreementText { get; set; }

    public bool IsTaxExempt { get; set; }

    public int TaxCategoryId { get; set; }

    public bool CallForPrice { get; set; }

    public decimal Price { get; set; }

    public decimal ProductCost { get; set; }

    public DateTime? AvailableStartDateTimeUtc { get; set; }

    public DateTime? AvailableEndDateTimeUtc { get; set; }

    public int DisplayOrder { get; set; }

    public bool Published { get; set; }

    public bool Deleted { get; set; }

    public int ProductTypeId { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime UpdatedOnUtc { get; set; }

    public int? FormId { get; set; }

    public string? RedirectUrl { get; set; }

    public virtual Form? Form { get; set; }

    public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<ProductCategoryMapping> ProductCategoryMappings { get; set; } = new List<ProductCategoryMapping>();

    public virtual ProductType ProductType { get; set; } = null!;

    public virtual ICollection<Traceability> Traceabilities { get; set; } = new List<Traceability>();

    public virtual ICollection<Form> Forms { get; set; } = new List<Form>();
}
