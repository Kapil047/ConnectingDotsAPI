using ConnectingDotsAPI.DBModels;
using ConnectingDotsAPI.Models;

namespace ConnectingDotsAPI.Services.PropertyService
{
    public interface IPropertyService
    {
        Task<Property> Delete(Guid guid);
        Task<List<PropertyModel.PropertyDetails>> GetAll();
        PropertyModel.PropertyDetails? GetDetails(Guid? Guid, int? Id);
        Task<Property> Save(PropertyModel.PropertyRequest request);
    }
}