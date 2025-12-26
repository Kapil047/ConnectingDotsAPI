using ConnectingDotsAPI.DBModels;
using ConnectingDotsAPI.Models;
using static ConnectingDotsAPI.Models.UserModel;

namespace ConnectingDotsAPI.Services.UserService
{
    public interface IUserService
    {
        Task ChangePassword(AuthModel.UpdatePasswordRequest request);
        Task<List<UserDetails>> GetAll(int userId, string? role);
        Task<UserDetails?> GetDetails(Guid? guid, int? id);
        Task<List<UserModel.RoleDetails>> GetRoles(int userId);
        Task<User> Save(UserModel.IUserSaveRequest request);
        void UpdateRole(UserModel.UpdateRoleRequest request);
        Task<int> Delete(Guid guid);
        Task<UserRole> SaveRole(SaveRoleRequest request);
        Task<UserRole> UpdateRoleAssignment(UpdateRoleAssigment request);
        RoleDetails GetRoleDetails(int roleId);
    }
}