using ConnectingDotsAPI.DBModels;
using System.ComponentModel.DataAnnotations;
using static ConnectingDotsAPI.Models.UserModel;

namespace ConnectingDotsAPI.Models
{
    public class CustomerModel
    {
        public class CustomerDetails
        {
            public int Id { get; set; }
            public string? Username { get; set; }
            public string? Email { get; set; }
            public string? EmailToRevalidate { get; set; }
            public string? SystemName { get; set; }
            public int? BillingAddressId { get; set; }
            public int? ShippingAddressId { get; set; }
            public Guid Guid { get; set; }
            public string? AdminComment { get; set; }
            public bool IsTaxExempt { get; set; }
            public int AffiliateId { get; set; }
            public int FailedLoginAttempts { get; set; }
            public DateTime? CannotLoginUntilDateUtc { get; set; }
            public bool Active { get; set; }
            public bool IsSystemAccount { get; set; }
            public string? LastIpAddress { get; set; }
            public DateTime CreatedOnUtc { get; set; }
            public DateTime? LastLoginDateUtc { get; set; }
            public DateTime LastActivityDateUtc { get; set; }
            public AddressDetails? BillingAddress { get; set; }
            public string? Password { get; set; } = string.Empty;
            public AddressDetails? ShippingAddress { get; set; }
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public object? Attributes { get; set; }
        }
        public partial class AddressDetails
        {
            public int Id { get; set; }
            public int? CountryId { get; set; }
            public int? StateProvinceId { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? Email { get; set; }
            public string? Company { get; set; }
            public string? City { get; set; }
            public string? Address1 { get; set; }
            public string? Address2 { get; set; }
            public string? ZipPostalCode { get; set; }
            public string? PhoneNumber { get; set; }
            public string? FaxNumber { get; set; }
            public string? CustomAttributes { get; set; }
            public CountryDetails? Country { get; set; }
            public StateProvinceDetails? StateProvince { get; set; }
        }
        public class CustomerRequest
        {
            public string? Guid { get; set; }
            public string? Email { get; set; }
            public string? EmailToRevalidate { get; set; }
            public string? Username { get; set; }
            public string? AdminComment { get; set; }
            public bool? IsTaxExempt { get; set; }
            public bool? Active { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? Password { get; set; }
            public string? Company { get; set; }
            public Dictionary<string, string>? Attributes { get; set; }
        }


        public class CountryDetails
        {
            public int? Id { get; set; }
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
            public int NumberOfStates { get; set; }
            public object? States { get; set; }
        }
        public class StateProvinceDetails
        {
            public int? Id { get; set; }
            public string Name { get; set; } = null!;
            public string? Abbreviation { get; set; }
            public int CountryId { get; set; }
            public bool Published { get; set; }
            public int DisplayOrder { get; set; }
        }
        public class CountryRequest
        {
            public int? Id { get; set; }
            public string Name { get; set; } = null!;
            public string? TwoLetterIsoCode { get; set; }
            public string? ThreeLetterIsoCode { get; set; }
            public bool AllowsBilling { get; set; }
            public bool AllowsShipping { get; set; }
            public int NumericIsoCode { get; set; }
            public bool Published { get; set; }
            public int DisplayOrder { get; set; }
        }
        public class StateRequest
        {
            public int? Id { get; set; }
            public string Name { get; set; } = null!;
            public string Abbreviation { get; set; } = null!;
            public int CountryId { get; set; }
            public bool Published { get; set; }
            public int DisplayOrder { get; set; }
        }

        public class AddressRequest
        {
            public int? Id { get; set; }
            public int? CountryId { get; set; }
            public int? StateProvinceId { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? Email { get; set; }
            public string? Company { get; set; }
            public string? Country { get; set; }
            public string? City { get; set; }
            public string? Address1 { get; set; }
            public string? Address2 { get; set; }
            public string? ZipPostalCode { get; set; }
            public string? PhoneNumber { get; set; }
            public int CustomerId { get; set; }

        }
        public class MapAddressRequest
        {
            public int CustomerId { get; set; }
            public int AddressId { get; set; }
            public AddressType AddressType { get; set; }
        }
        public class DeleteAddressRequest
        {
            public int CustomerId { get; set; }
            public int AddressId { get; set; }
        }

        #region ENUMS
        public enum AddressType
        {
            BillingAddress,
            ShippingAddress,
        }
        #endregion

    }
}
