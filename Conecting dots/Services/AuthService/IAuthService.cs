using ConnectingDotsAPI.Models;
using ConnectingDotsAPI.Models.Auth;

namespace ConnectingDotsAPI.Services
{
    public interface IAuthService
    {
        int GetCustomerId(string jwtToken);

        //int GetCustomerId(string jwtToken);
        int GetUserId(string jwtToken);
        bool IsCustomerTokenValid(string token);
        bool IsTokenValid(string token);
        Task<AuthModel.AuthResult> Login(LoginModel request);
        Task Register(AuthModel.RegisterRequest request);
    }
}