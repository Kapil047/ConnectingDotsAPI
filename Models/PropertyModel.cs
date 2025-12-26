using System.ComponentModel.DataAnnotations;

namespace ConnectingDotsAPI.Models
{
    public class PropertyModel
    {
        public class PropertyDetails
        {
            public Guid Guid { get; set; }
            public string PropertyName { get; set; }
            public string PropertyType { get; set; }
            public string AddressLine1 { get; set; }
            public string AddressLine2 { get; set; }
            public string City { get; set; }
            public object State { get; set; }
            public string PostalCode { get; set; }
            public object Country { get; set; }
            public decimal Price { get; set; }
            public int Bedrooms { get; set; }
            public int Bathrooms { get; set; }
            public int SquareFeet { get; set; }
            public string Description { get; set; }
            public DateTime ListingDate { get; set; }
            public bool IsActive { get; set; } = true;
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
        }
        public class PropertyRequest
        {
            public string? Guid { get; set; }

            [Required]
            [MaxLength(255)]
            public string PropertyName { get; set; }

            [Required]
            [MaxLength(100)]
            [RegularExpression("Commercial|Land|Condo|House|Apartment", ErrorMessage = "PropertyType must be one of the following: Commercial, Land, Condo, House, Apartment")]
            public string PropertyType { get; set; }

            [Required]
            [MaxLength(255)]
            public string AddressLine1 { get; set; }

            [MaxLength(255)]
            public string AddressLine2 { get; set; }

            [Required]
            [MaxLength(100)]
            public string City { get; set; }

            [Required]
            public int State { get; set; }

            [Required]
            [MaxLength(20)]
            public string PostalCode { get; set; }

            [Required]
            public int Country { get; set; }

            [Required]
            [Range(0, double.MaxValue)]
            public decimal Price { get; set; }

            [Required]
            [Range(0, int.MaxValue)]
            public int Bedrooms { get; set; }

            [Required]
            [Range(0, int.MaxValue)]
            public int Bathrooms { get; set; }

            [Required]
            [Range(0, int.MaxValue)]
            public int SquareFeet { get; set; }

            public string Description { get; set; }

            public DateTime ListingDate { get; set; } = DateTime.Now;

            public bool IsActive { get; set; } = true;

            public DateTime CreatedAt { get; set; } = DateTime.Now;

            public DateTime? UpdatedAt { get; set; }

            public bool Deleted { get; set; } = false;
        }

    }
}
