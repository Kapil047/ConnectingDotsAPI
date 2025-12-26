using System.ComponentModel.DataAnnotations;

namespace ConnectingDotsAPI.Models
{
    public class SettingsModel
    {
        public class ConfigurationRequest
        {
            public string Name { get; set; } = null!;
            public string Value { get; set; } = null!;
        }
        public class SettingsDetails
        {
            public int Id { get; set; }
            public required string Name { get; set; }
            public required string Value { get; set; }
            public int? StoreId { get; set; }
        }

        public class SitePageDetails
        {
            [Required]
            public int Id { get; set; }
            [Required]
            public required string Name { get; set; }
            [Required]
            public required string SystemName { get; set; }
        }
        public class PagesInRolesRequest
        {
            [Required]
            public int RoleId { get; set; }
            [Required]
            public required List<int> PagesId { get; set; }
        }
        public class CountryRequest
        {
            public int? Id { get; set; }
            [Required]
            public string Name { get; set; } = null!;
            public string? TwoLetterIsoCode { get; set; }
            public string? ThreeLetterIsoCode { get; set; }
            public bool AllowsBilling { get; set; }
            public bool AllowsShipping { get; set; }
            public int NumericIsoCode { get; set; }
            public bool Published { get; set; }
            public int DisplayOrder { get; set; }
        }
        public class CountryDetails
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
            public string? TwoLetterIsoCode { get; set; }
            public string? ThreeLetterIsoCode { get; set; }
            public bool AllowsBilling { get; set; }
            public bool AllowsShipping { get; set; }
            public int NumericIsoCode { get; set; }
            public bool Published { get; set; }
            public int DisplayOrder { get; set; }
            public int NumberOfStates { get; set; }
            public object? States { get; set; }
        }
        public class StateRequest
        {
            public int? Id { get; set; }
            [Required]
            public string Name { get; set; } = null!;
            public string Abbreviation { get; set; } = null!;
            [Required]
            public int CountryId { get; set; }
            public bool Published { get; set; }
            public int DisplayOrder { get; set; }
        }
         public class CityRequest
        {
            public int? Id { get; set; }
            [Required]
            public string Name { get; set; } = null!;
            public int StateId { get; set; }
            public int CountryId { get; set; }
            public bool Published { get; set; }
            public int DisplayOrder { get; set; } = 1;
        }

        public partial class EmailTemplateRequest
        {
            public int? Id { get; set; }
            public string Name { get; set; } = null!;
            public string? Subject { get; set; }
            public string? SendTo { get; set; }
            public string SystemName { get; set; } = null!;
            public string TemplateHtml { get; set; } = null!;
            public bool Active { get; set; }
            public bool PreviewFirst { get; set; }
        }
    }
}
