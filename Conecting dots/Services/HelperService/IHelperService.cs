
using ConnectingDotsAPI.DBModels;

namespace ConnectingDotsAPI.Services.HelperService
{
    public interface IHelperService
    {
        Task DeactivateUrlRecord(int entityId, string entityName);
        Task DeleteUrlRecord(int entityId, string entityName);
        int? FindByReferenceCodeAndType(string type, string code);
        int? GetEntityId(string entityName, string slug);
        List<ReferenceCode> GetReferenceCodes(string type);
        Task<int> SaveReferenceType(string code);
        Task UpdateUrlRecord(int entityId, string entityName, string slug);
        int? FindByReferenceType(string type);
        Task<ReferenceCode> DeleteReferenceCode(string code);
        ReferenceCode GetReferenceCodeDetails(string type, string code);
        Task<ReferenceCode> SaveReferenceCode(int referenceTypeId, string systemKeyword, string name);
    }
}