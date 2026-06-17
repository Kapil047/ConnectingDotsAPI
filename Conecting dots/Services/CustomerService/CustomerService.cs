using ConnectingDotsAPI.DBModels;
using ConnectingDotsAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;
using static ConnectingDotsAPI.Models.CustomerModel;
using static ConnectingDotsAPI.Models.UserModel;

namespace ConnectingDotsAPI.Services.CustomerService
{
    public class CustomerService(ConnectingDotsDbContext db) : ICustomerService
    {
        private readonly ConnectingDotsDbContext db = db;
        public async Task<List<CustomerDetails>> GetAll()
        {
            return await db.Customers.Where(x => !x.Deleted)
                      .Select(Customer => new CustomerDetails
                      {
                          Active = Customer.Active,
                          AdminComment = Customer.AdminComment,
                          AffiliateId = Customer.AffiliateId,
                          BillingAddressId = Customer.BillingAddressId,
                          CreatedOnUtc = Customer.CreatedOnUtc,
                          Guid = Customer.Guid,
                          Email = Customer.Email,
                          EmailToRevalidate = Customer.EmailToRevalidate,
                          Id = Customer.Id,
                          FirstName = Customer.FirstName ?? "",
                          LastName = Customer.LastName ?? "",
                          Attributes = db.GenericAttributes.Where(x => x.KeyGroup.ToUpper().Equals("CUSTOMER")
      && x.EntityId == Customer.Id && (x.Key == "Mobile" || x.Key=="Photo")).Select(x => new { x.Key, x.Value }).ToList(),
                      }).ToListAsync();
        }
        public CustomerDetails? GetDetails(Guid? CustomerGuid, int? CustomerId)
        {
            var Customer = db.Customers.Where(x => x.Guid == CustomerGuid || x.Id == CustomerId)
    .Include(x => x.BillingAddress)
    .Include(x => x.ShippingAddress)
    .Select(c => new CustomerDetails
    {
        Active = c.Active,
        AffiliateId = c.AffiliateId,
        BillingAddressId = c.BillingAddressId,
        BillingAddress = c.BillingAddress != null ? new AddressDetails
        {
            Address1 = c.BillingAddress.Address1,
            Address2 = c.BillingAddress.Address2,
            City = c.BillingAddress.City,
            Country = new CountryDetails
            {
                Id = c.BillingAddress.CountryId,
                Name = c.BillingAddress.Country.Name,
                ThreeLetterIsoCode = c.BillingAddress.Country.ThreeLetterIsoCode,
                TwoLetterIsoCode = c.BillingAddress.Country.TwoLetterIsoCode
            },
            Company = c.BillingAddress.Company,
            Email = c.BillingAddress.Email,
            CountryId = c.BillingAddress.CountryId,
            FirstName = c.BillingAddress.FirstName,
            CustomAttributes = c.BillingAddress.CustomAttributes,
            FaxNumber = c.BillingAddress.FaxNumber,
            LastName = c.BillingAddress.LastName,
            PhoneNumber = c.BillingAddress.PhoneNumber,
            StateProvinceId = c.BillingAddress.StateProvinceId,
            StateProvince = new StateProvinceDetails
            {
                Id = c.BillingAddress.StateProvinceId,
                Abbreviation = c.BillingAddress.StateProvince.Abbreviation,
                CountryId = c.BillingAddress.StateProvince.CountryId,
                Name = c.BillingAddress.StateProvince.Name
            },
            ZipPostalCode = c.BillingAddress.ZipPostalCode,
        } : null,
        ShippingAddress = c.ShippingAddress != null ? new AddressDetails
        {
            Address1 = c.ShippingAddress.Address1,
            Address2 = c.ShippingAddress.Address2,
            City = c.ShippingAddress.City,
            Country = new CountryDetails
            {
                Id = c.ShippingAddress.CountryId,
                Name = c.ShippingAddress.Country.Name,
                ThreeLetterIsoCode = c.ShippingAddress.Country.ThreeLetterIsoCode,
                TwoLetterIsoCode = c.ShippingAddress.Country.TwoLetterIsoCode
            },
            Company = c.ShippingAddress.Company,
            Email = c.ShippingAddress.Email,
            CountryId = c.ShippingAddress.CountryId,
            FirstName = c.ShippingAddress.FirstName,
            CustomAttributes = c.ShippingAddress.CustomAttributes,
            FaxNumber = c.ShippingAddress.FaxNumber,
            LastName = c.ShippingAddress.LastName,
            PhoneNumber = c.ShippingAddress.PhoneNumber,
            StateProvinceId = c.ShippingAddress.StateProvinceId,
            StateProvince = new StateProvinceDetails
            {
                Id = c.ShippingAddress.StateProvinceId,
                Abbreviation = c.ShippingAddress.StateProvince.Abbreviation,
                CountryId = c.ShippingAddress.StateProvince.CountryId,
                Name = c.ShippingAddress.StateProvince.Name
            },
            ZipPostalCode = c.ShippingAddress.ZipPostalCode,
        } : null,
        AdminComment = c.AdminComment,
        CannotLoginUntilDateUtc = c.CannotLoginUntilDateUtc,
        CreatedOnUtc = c.CreatedOnUtc,
        Guid = c.Guid,
        Email = c.Email,
        EmailToRevalidate = c.EmailToRevalidate,
        FailedLoginAttempts = c.FailedLoginAttempts,
        Id = c.Id,
        IsSystemAccount = c.IsSystemAccount,
        IsTaxExempt = c.IsTaxExempt,
        LastActivityDateUtc = c.LastActivityDateUtc,
        LastIpAddress = c.LastIpAddress,
        LastLoginDateUtc = c.LastLoginDateUtc,
        ShippingAddressId = c.ShippingAddressId,
        SystemName = c.SystemName,
        Username = c.Username,
        FirstName = c.FirstName ?? "",
        LastName = c.LastName ?? "",
        Password = c.CustomerPasswords.OrderByDescending(x => x.Id).Select(x => x.Password).FirstOrDefault(),
        Attributes = db.GenericAttributes.Where(x => x.KeyGroup.ToUpper().Equals("CUSTOMER")
        && x.EntityId == c.Id).Select(x => new { x.Key, x.Value }).ToList(),
    })
    .FirstOrDefault() ?? throw new Exception("NOT_FOUND");


            return Customer;

        }
        public async Task<Customer> SaveCustomer(CustomerRequest request)
        {

            if (string.IsNullOrEmpty(request.Guid) && !string.IsNullOrEmpty(request.Email))
            {
                if (db.Customers.Any(x => !x.Deleted && x.Email.ToLower() == request.Email.ToLower().Trim()))
                    throw new Exception("DUPLICATE_EMAIL");
            }

            var val = new Customer() { Guid = Guid.NewGuid() };
            if (!string.IsNullOrEmpty(request.Guid))
                val = db.Customers.FirstOrDefault(c => c.Guid == Guid.Parse(request.Guid));
            if (val == null) throw new Exception();
            if (request.IsTaxExempt.HasValue)
                val.IsTaxExempt = request.IsTaxExempt.Value;
            if (request.Active.HasValue)
                val.Active = request.Active.Value;
            if (!string.IsNullOrEmpty(request.FirstName))
                val.FirstName = request.FirstName;
            if (!string.IsNullOrEmpty(request.LastName))
                val.LastName = request.LastName;

            if (!string.IsNullOrEmpty(request.AdminComment))
                val.AdminComment = request.AdminComment;
            if (!string.IsNullOrEmpty(request.Email))
                val.Email = request.Email;

            if (!string.IsNullOrEmpty(request.EmailToRevalidate))
                val.EmailToRevalidate = request.EmailToRevalidate;

            if (val.Id == 0)
            {
                val.CreatedOnUtc = DateTime.UtcNow;
                val.Guid = Guid.NewGuid();
                db.Customers.Add(val);


            }
            await db.SaveChangesAsync();

            if (string.IsNullOrEmpty(request.Guid))
            {
                db.CustomerPasswords.Add(new CustomerPassword
                {
                    Password = request.Password,
                    CreatedOnUtc = DateTime.UtcNow,
                    CustomerId = val.Id,

                });





                //commented this as firstname and lastname are now added to the Customer table. in future if further GA needs to be added, it can be enabled.

                //db.GenericAttributes.Add(new GenericAttribute
                //{
                //    CreatedOrUpdatedDateUtc = DateTime.UtcNow,
                //    EntityId = val.Id,
                //    Key = "FirstName",
                //    KeyGroup = "Customer",
                //    Value = request.FirstName ?? "",
                //});
                //if (!string.IsNullOrEmpty(request.Gender))
                //    db.GenericAttributes.Add(new GenericAttribute
                //    {
                //        CreatedOrUpdatedDateUtc = DateTime.UtcNow,
                //        EntityId = val.Id,
                //        Key = "Gender",
                //        KeyGroup = "Customer",
                //        Value = request.LastName ?? "",
                //    });
                //if (!string.IsNullOrEmpty(request.Phone))
                //    db.GenericAttributes.Add(new GenericAttribute
                //    {
                //        CreatedOrUpdatedDateUtc = DateTime.UtcNow,
                //        EntityId = val.Id,
                //        Key = "Phone",
                //        KeyGroup = "Customer",
                //        Value = request.Phone,
                //    });
                //disabling company as it is not required generally. it can be enabled if making for a corporate
                //db.GenericAttributes.Add(new GenericAttribute
                //{
                //    CreatedOrUpdatedDateUtc = DateTime.UtcNow,
                //    EntityId = val.Id,
                //    Key = "Company",
                //    KeyGroup = "Customer",
                //    Value = request.Company ?? "",
                //});
            }
            //else
            //{
            //    if (!string.IsNullOrEmpty(request.Phone))
            //    {
            //        var _ga = db.GenericAttributes.FirstOrDefault(ga => ga.KeyGroup == "Customer"
            //         && ga.EntityId == val.Id && ga.Key == "Phone"
            //         ) ?? new GenericAttribute();
            //        _ga.Value = request.Phone;
            //        _ga.KeyGroup = "Customer";
            //        _ga.CreatedOrUpdatedDateUtc = DateTime.Now;
            //        _ga.EntityId = val.Id;
            //        _ga.Key = "Phone";
            //        if (_ga.Id == 0)
            //        {
            //            db.GenericAttributes.Add(_ga);
            //        }
            //    }
            //    if (!string.IsNullOrEmpty(request.Gender))
            //    {
            //        var _ga = db.GenericAttributes.FirstOrDefault(ga => ga.KeyGroup == "Customer"
            //         && ga.EntityId == val.Id && ga.Key == "Gender"
            //         ) ?? new GenericAttribute();
            //        _ga.Value = request.Gender;
            //        _ga.KeyGroup = "Customer";
            //        _ga.CreatedOrUpdatedDateUtc = DateTime.Now;
            //        _ga.EntityId = val.Id;
            //        _ga.Key = "Gender";
            //        if (_ga.Id == 0)
            //        {
            //            db.GenericAttributes.Add(_ga);
            //        }
            //    }
            //    //if (!string.IsNullOrEmpty(request.LastName))
            //    //{
            //    //    var _ga = db.GenericAttributes.FirstOrDefault(ga => ga.KeyGroup == "Customer"
            //    //     && ga.EntityId == val.Id && ga.Key == "LastName"
            //    //     ) ?? new GenericAttribute();
            //    //    _ga.Value = request.LastName;
            //    //    _ga.KeyGroup = "Customer";
            //    //    _ga.CreatedOrUpdatedDateUtc = DateTime.Now;
            //    //    _ga.EntityId = val.Id;
            //    //    _ga.Key = "LastName";
            //    //    if (_ga.Id == 0)
            //    //    {
            //    //        db.GenericAttributes.Add(_ga);
            //    //    }
            //    //}

            //}

            #region Key value pairs for generic attribues
            if (request.Attributes != null && request.Attributes.Count > 0)
            {
                foreach (var kvp in request.Attributes)
                {
                    var attribute = db.GenericAttributes.FirstOrDefault(x => x.Key == kvp.Key && x.KeyGroup == "Customer" && x.EntityId == val.Id);
                    if (attribute != null)
                    {
                        attribute.Value = kvp.Value;
                    }
                    else
                    {
                        db.GenericAttributes.Add(new GenericAttribute
                        {
                            CreatedOrUpdatedDateUtc = DateTime.UtcNow,
                            EntityId = val.Id,
                            Key = kvp.Key,
                            KeyGroup = "Customer",
                            Value = kvp.Value,
                        });
                    }
                }
            }
            #endregion
            await db.SaveChangesAsync();
            return new Customer { Id = val.Id, Guid = val.Guid };
        }
        public AddressDetails GetAddressDetails(int id)
        {
            return db.Addresses.Where(a => a.Id == id).Include(x => x.StateProvince).Select(x => new AddressDetails
            {
                Address1 = x.Address1,
                Address2 = x.Address2,
                City = x.City,
                Company = x.Company,
                CountryId = x.CountryId,
                Email = x.Email,
                FirstName = x.FirstName,
                LastName = x.LastName,
                PhoneNumber = x.PhoneNumber,
                StateProvinceId = x.StateProvinceId,
                ZipPostalCode = x.ZipPostalCode,
                StateProvince = x.StateProvince != null ? new StateProvinceDetails
                {
                    Abbreviation = x.StateProvince.Abbreviation,
                    CountryId = x.StateProvince.CountryId,
                    DisplayOrder = x.StateProvince.DisplayOrder,
                    Id = x.Id,
                    Published = x.StateProvince.Published,
                    Name = x.StateProvince.Name
                } : null,
                Id = x.Id
            }).First();

        }
        public async Task ChangeCustomerPassword(AuthModel.UpdatePasswordRequest request)
        {
            var Customer = db.Customers.FirstOrDefault(x => x.Guid == Guid.Parse(request.Id))
                ?? throw new Exception("NOT_FOUND");
            db.CustomerPasswords.Add(new CustomerPassword
            {
                CreatedOnUtc = DateTime.UtcNow,
                Password = request.Password,
                CustomerId = Customer.Id
            });
            db.CustomerAuthTokens.RemoveRange(db.CustomerAuthTokens.Where(x => x.CustomerId == Customer.Id));
            await db.SaveChangesAsync();
        }
        public async Task<int> DeleteCustomer(Guid CustomerGuid)
        {
            var c = db.Customers.FirstOrDefault(x => x.Guid == CustomerGuid) ?? throw new Exception("Customer_NOT_FOUND");
            c.Deleted = true;
            await db.SaveChangesAsync();
            return c.Id;
        }
        public async Task SaveAddress(AddressRequest request)
        {
            var val = db.Addresses.FirstOrDefault(a => a.Id == request.Id) ?? new Address();
            val.FirstName = request.FirstName;
            val.LastName = request.LastName;
            val.PhoneNumber = request.PhoneNumber;
            val.Company = request.Company;
            val.Address1 = request.Address1;
            val.Address2 = request.Address2;
            val.City = request.City;
            val.CountryId = request.CountryId;
            val.Email = request.Email;
            val.StateProvinceId = request.StateProvinceId;
            val.ZipPostalCode = request.ZipPostalCode;

            if (val.Id == 0)
            {
                val.CreatedOnUtc = DateTime.UtcNow;
                db.Addresses.Add(val);
                //var Customer = db.Customers.Where(c => c.Id == request.CustomerId).Include(x=>x.);
                //Customer?.Addresses.Add(val);
            }
            await db.SaveChangesAsync();

        }
        public async Task UpdateBillingAddressId(int CustomerId, int addressId)
        {
            var Customer = db.Customers.FirstOrDefault(c => c.Id == CustomerId);
            if (Customer != null)
            {
                Customer.BillingAddressId = addressId;
                await db.SaveChangesAsync();
            }
        }
        public async Task UpdateShippingAddressId(int CustomerId, int addressId)
        {
            var Customer = db.Customers.FirstOrDefault(c => c.Id == CustomerId);
            if (Customer != null)
            {
                Customer.ShippingAddressId = addressId;
                await db.SaveChangesAsync();
            }
        }
        public async Task MapCustomerAddress(MapAddressRequest request)
        {
            var Customer = db.Customers.First(c => c.Id == request.CustomerId);
            if (Customer == null) throw new Exception("Customer_NOT_FOUND");
            if (request.AddressType == AddressType.BillingAddress)
                Customer.BillingAddressId = request.AddressId;
            if (request.AddressType == AddressType.ShippingAddress)
                Customer.ShippingAddressId = request.AddressId;
            await db.SaveChangesAsync();
        }
        public void DeleteAddress(DeleteAddressRequest request)
        {
            db.Database.ExecuteSql($"DELETE FROM [CustomerAddresses] WHERE Customer_Id={request.CustomerId} AND Address_Id={request.AddressId}");
        }


       
    }
}
