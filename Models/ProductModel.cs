using System.ComponentModel.DataAnnotations;

namespace ConnectingDotsAPI.Models
{
    public class ProductModel
    {
        #region Product
        public class ProductCategoryRequest
        {
            public string? Guid { get; set; }
            public string Name { get; set; } = null!;
            public string? MetaKeywords { get; set; }
            public string? MetaTitle { get; set; }
            public string? Description { get; set; }
            public string? MetaDescription { get; set; }
            public int ParentCategoryId { get; set; }
            public int PictureId { get; set; }
            public bool ShowOnHomepage { get; set; }
            public bool IncludeInTopMenu { get; set; }
            public bool Published { get; set; }
            public int DisplayOrder { get; set; } = 0;
            public string Slug { get; set; } = null!;
            public string? FullDescription { get; set; }

        }
        public class MapProductFormRequest
        {
            public required string FormId { get; set; }
            public required string ProductId { get; set; }
            public UserModel.MethodType Action { get; set; }
        }
        public class MapProductCategoryRequest
        {
            public required string CategoryGuid { get; set; }
            public required string ProductGuid { get; set; }
            public int? DisplayOrder { get; set; }
            public bool? IsFeaturedProduct { get; set; }
            public UserModel.MethodType Action { get; set; }
        }
        public class ProductCategoryDetails
        {
            public int Id { get; set; }
            public required Guid Guid { get; set; }
            public string Name { get; set; } = null!;
            public string? Slug { get; set; }
            public string? MetaKeywords { get; set; }
            public string? MetaTitle { get; set; }
            public string? Description { get; set; }
            public string? MetaDescription { get; set; }
            public int ParentCategoryId { get; set; }
            public FileModel.FileDetails? Picture { get; set; }
            public FileModel.FileDetails? Icon { get; set; }
            public bool ShowOnHomepage { get; set; }
            public bool IncludeInTopMenu { get; set; }
            public bool Published { get; set; }
            public int DisplayOrder { get; set; }
            public object? Products { get; set; }
            public object? Categories { get; set; }
            public string? FullDescription { get; set; }
        }

        public class ProductRequest
        {
            public string? Guid { get; set; }
            [Required]
            [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
            public string Name { get; set; } = null!;

            [StringLength(255, ErrorMessage = "Meta keywords cannot exceed 255 characters.")]
            public string? MetaKeywords { get; set; }

            [StringLength(255, ErrorMessage = "Meta title cannot exceed 255 characters.")]
            public string? MetaTitle { get; set; }

            [StringLength(50, ErrorMessage = "SKU cannot exceed 50 characters.")]
            public string? Sku { get; set; }

            [StringLength(500, ErrorMessage = "Short description cannot exceed 500 characters.")]
            public string? ShortDescription { get; set; }

            public string? FullDescription { get; set; }

            [StringLength(1000, ErrorMessage = "Admin comment cannot exceed 1000 characters.")]
            public string? AdminComment { get; set; }

            public bool ShowOnHomepage { get; set; }

            [StringLength(255, ErrorMessage = "Meta description cannot exceed 255 characters.")]
            public string? MetaDescription { get; set; }

            public bool HasUserAgreement { get; set; }

            public string? UserAgreementText { get; set; }

            public bool IsTaxExempt { get; set; }

            [Range(1, int.MaxValue, ErrorMessage = "Please enter a valid tax category ID.")]
            public int TaxCategoryId { get; set; }

            [Range(0.0, double.MaxValue, ErrorMessage = "Please enter a valid price.")]
            public decimal Price { get; set; }

            [Range(0.0, double.MaxValue, ErrorMessage = "Please enter a valid product cost.")]
            public decimal ProductCost { get; set; }

            public string? HSN { get; set; }
            [Range(int.MinValue, int.MaxValue, ErrorMessage = "Please enter a valid display order.")]
            public int DisplayOrder { get; set; }
            [Range(1, int.MaxValue, ErrorMessage = "Please enter a valid product type ID.")]
            public int ProductTypeId { get; set; }

            public string? FormId { get; set; }

            public bool Published { get; set; }
            public string Slug { get; set; } = null!;
            [StringLength(500, ErrorMessage = "Redirect Url cannot exceed 500 characters.")]
            public string? RedirectUrl { get; set; }

        }
        public class ProductDetails
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = null!;
            public string? Sku { get; set; }
            public object? Form { get; set; }
            public string? ShortDescription { get; set; }
            public string? FullDescription { get; set; }
            public string? AdminComment { get; set; }
            public string? MetaDescription { get; set; }
            public string? MetaKeywords { get; set; }
            public bool HasUserAgreement { get; set; }
            public string? UserAgreementText { get; set; }
            public bool IsTaxExempt { get; set; }
            public int TaxCategoryId { get; set; }
            public decimal Price { get; set; }
            public decimal ProductCost { get; set; }
            public object ProductType { get; set; } = null!;
            public object Forms { get; set; } = null!;
            public int DisplayOrder { get; set; }
            public bool Published { get; set; }
            public string Slug { get; set; } = null!;
            public FileModel.FileDetails? Picture { get; set; }
            public List<FileModel.FileDetails>? Attachments { get; set; }
            public object? Categories { get; set; }
            public string? RedirectUrl { get; set; }
        }
        public class ReferenceCodeRequest
        {
            public int Id { get; set; }
            public int ReferenceTypeId { get; set; }
            public string? SystemKeyword { get; set; }
            public string Name { get; set; } = null!;
            public bool Enabled { get; set; }
        }
        #endregion

        #region Attachments
        public class ImageUploadRequest
        {
            public required string Guid { get; set; }
            public string Type { get; set; } = null!;
            public string Url { get; set; } = null!;
            public string FileName { get; set; } = null!;
            public AttachmentType AttachmentType { get; set; }
        }
        public class AttachmentsRequest
        {
            public int Id { get; set; }
            public string Type { get; set; } = null!;
        }
        public enum AttachmentType
        {
            Image,
            Icon,
            Attachment
        }
        #endregion
    }
}
