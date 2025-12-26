using ConnectingDotsAPI.DBModels;
using ConnectingDotsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ConnectingDotsAPI.Services.PropertyService
{
    public class PropertyService(ConnectingDotsDbContext db) : IPropertyService
    {
        private readonly ConnectingDotsDbContext db = db;

        public async Task<List<PropertyModel.PropertyDetails>> GetAll()
        {
            return await db.Properties.Where(x => !x.Deleted)
                      .Select(value => new PropertyModel.PropertyDetails
                      {
                          Guid = value.Guid,
                          Bathrooms = value.Bathrooms,
                          Bedrooms = value.Bedrooms,
                          City = value.City,
                          CreatedAt = value.CreatedAt,
                          IsActive = value.IsActive,
                          ListingDate = value.ListingDate,
                          Price = value.Price,
                          PropertyName = value.PropertyName,
                          PropertyType = value.PropertyType,
                          SquareFeet = value.SquareFeet,
                          State = value.StateNavigation.Name,
                          UpdatedAt = value.UpdatedAt,
                      }).ToListAsync();
        }
        public PropertyModel.PropertyDetails? GetDetails(Guid? Guid, int? Id)
        {
            return db.Properties.Where(x => x.Guid == Guid || x.Id == Id)

                     .Select(value => new PropertyModel.PropertyDetails
                     {
                         Guid = value.Guid,
                         AddressLine1 = value.AddressLine1,
                         AddressLine2 = value.AddressLine2 ?? "",
                         Bathrooms = value.Bathrooms,
                         Bedrooms = value.Bedrooms,
                         City = value.City,
                         Country = new { value.CountryNavigation.Name, value.CountryNavigation.Id },
                         CreatedAt = value.CreatedAt,
                         Description = value.Description ?? "",
                         IsActive = value.IsActive,
                         ListingDate = value.ListingDate,
                         PostalCode = value.PostalCode,
                         Price = value.Price,
                         PropertyName = value.PropertyName,
                         PropertyType = value.PropertyType,
                         SquareFeet = value.SquareFeet,
                         State = new
                         {
                             value.StateNavigation.Name,
                             value
                         .StateNavigation.Id
                         },
                         UpdatedAt = value.UpdatedAt,

                     }).FirstOrDefault();

        }
        public async Task<Property> Save(PropertyModel.PropertyRequest request)
        {

            var val = new Property() { Guid = Guid.NewGuid() };
            if (!string.IsNullOrEmpty(request.Guid))
                val = db.Properties.FirstOrDefault(c => c.Guid == Guid.Parse(request.Guid));
            if (val == null) throw new Exception();

            val.AddressLine1 = request.AddressLine1;
            val.AddressLine2 = request.AddressLine2;
            val.Bathrooms = request.Bathrooms;
            val.Bedrooms = request.Bedrooms;
            val.City = request.City;
            val.Country = request.Country;
            val.Description = request.Description;
            val.IsActive = request.IsActive;
            val.ListingDate = request.ListingDate;
            val.PostalCode = request.PostalCode;
            val.Price = request.Price;
            val.PropertyName = request.PropertyName;
            val.PropertyType = request.PropertyType;
            val.SquareFeet = request.SquareFeet;
            val.State = request.State;
            val.UpdatedAt = DateTime.Now;

            if (val.Id == 0)
            {
                val.CreatedAt = DateTime.UtcNow;
                db.Properties.Add(val);
            }
            await db.SaveChangesAsync();
            return new Property { Id = val.Id, Guid = val.Guid };
        }
        public async Task<Property> Delete(Guid guid)
        {
            var val = db.Properties.FirstOrDefault(x => x.Guid == guid)
                ?? throw new Exception("NOT_FOUND");
            val.Deleted = true;
            await db.SaveChangesAsync();
            return new Property { Id = val.Id, Guid = val.Guid };
        }
    }
}
