using ConnectingDotsAPI.DBModels;
using ConnectingDotsAPI.Models;
using ConnectingDotsAPI.Models;
using static ConnectingDotsAPI.Models.CustomerModel;

namespace ConnectingDotsAPI.Services.CustomerService
{
    public interface ICustomerService
    {
        Task ChangeCustomerPassword(AuthModel.UpdatePasswordRequest request);
        void DeleteAddress(CustomerModel.DeleteAddressRequest request);
        Task<int> DeleteCustomer(Guid CustomerGuid);
        CustomerModel.AddressDetails GetAddressDetails(int id);
        Task<List<CustomerModel.CustomerDetails>> GetAll();
        CustomerModel.CustomerDetails? GetDetails(Guid? CustomerGuid, int? CustomerId);
        Task MapCustomerAddress(CustomerModel.MapAddressRequest request);
        Task SaveAddress(CustomerModel.AddressRequest request);
       
        Task<Customer> SaveCustomer(CustomerModel.CustomerRequest request);
        Task UpdateBillingAddressId(int CustomerId, int addressId);
        Task UpdateShippingAddressId(int CustomerId, int addressId);
    }
}