using ConnectingDotsAPI.DBModels;
using ConnectingDotsAPI.Models;
using ConnectingDotsAPI.Models.Auth;
using ConnectingDotsAPI.Services.CacheService;
using ConnectingDotsAPI.Services.CustomerService;
using ConnectingDotsAPI.Services.UserService;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ConnectingDotsAPI.Services
{
    public class AuthService(ConnectingDotsDbContext db, IConfiguration configuration, ICustomerService customerService, ICacheService cacheService) : IAuthService
    {
        private readonly ConnectingDotsDbContext db = db;
        private readonly ICustomerService customerService = customerService;
        private readonly IConfiguration configuration = configuration;
        private readonly ICacheService cacheService = cacheService;

        public async Task<AuthModel.AuthResult> Login(LoginModel request)
        {

            if (request.Type == AuthModel.LoginType.Admin)
            {
                var user = db.Users
                    .Where(x => !string.IsNullOrEmpty(x.Username) && x.Active && !x.Deleted
                    && x.Username.Equals(request.Username.ToLower().Trim())).
                    Include(x => x.UserPasswords)
                    .Include(x => x.Roles).ThenInclude(x => x.PagesInRoles).ThenInclude(x => x.Page)
                    .FirstOrDefault();
                if (user != null)
                {
                    if (user.UserPasswords?.OrderByDescending(x => x.CreatedOnUtc)
                        .FirstOrDefault()?.Password == request.Password)
                    {
                        var _user = new AuthModel.JwtUserDetails
                        {
                            Id = user.Id.ToString(),
                            Guid = user.Guid.ToString(),
                            FirstName = user.FirstName,
                            LastName = user.LastName ?? "",
                            Roles = user.Roles.Select(_role =>
                            new
                            {
                                _role.Name,
                                _role.Id,
                                Pages = _role.PagesInRoles
                                .Select(_page => new
                                {
                                    id = _page.Id,
                                    name = _page.Page.SystemName
                                }).ToList()
                            })
                        };

                        var token = GenerateUserJSONWebToken(_user, user.Roles.Any(x => x.EnablePasswordLifetime));
                        db.UserAuthTokens.RemoveRange(db.UserAuthTokens.Where(x => x.UserId == user.Id));
                        user.UserAuthTokens.Add(new UserAuthToken { Token = token });

                        await db.SaveChangesAsync();
                        return new AuthModel.AuthResult { Message = "SUCCESS", Result = true, Token = token };
                    }
                    else
                    {
                        var result = new AuthModel.AuthResult { Message = "WRONG_PASSWORD", Result = false };
                        return result;
                    }
                }
                else
                {
                    var result = new AuthModel.AuthResult { Message = "INVALID", Result = false };
                    return result;
                }
            }
            else if (request.Type == AuthModel.LoginType.Customer)
            {
                var Customer = db.Customers
                    .Where(x => !string.IsNullOrEmpty(x.Email)
                    && x.Email.Equals(request.Username.ToLower().Trim())).
                    Include(x => x.CustomerPasswords)
                    .FirstOrDefault();
                if (Customer != null)
                {
                    if (Customer.CustomerPasswords?.OrderByDescending(x => x.CreatedOnUtc)
                        .FirstOrDefault()?.Password == request.Password)
                    {
                        //var details = customerService.GetDetails(null, Customer.Id) ?? throw new Exception("NO_Customer_DETAILS");
                        var _Customer = new AuthModel.JwtCustomerDetails
                        {
                            Email = Customer.Email ?? "",
                            Id = Customer.Guid.ToString(),
                            FirstName = Customer.FirstName ?? "",
                            LastName = Customer.LastName ?? "",
                        };
                        var token = GenerateJSONWebToken(_Customer);
                        db.CustomerAuthTokens.RemoveRange(db.CustomerAuthTokens.Where(x => x.CustomerId == Customer.Id));
                        db.CustomerAuthTokens.Add(new CustomerAuthToken
                        {
                            CustomerId = Customer.Id,
                            Token = token
                        });
                        await db.SaveChangesAsync();
                        return new AuthModel.AuthResult { Message = "SUCCESS", Result = true, Token = token };
                    }
                    else
                    {
                        var result = new AuthModel.AuthResult { Message = "WRONG_PASSWORD", Result = false };
                        return result;
                    }
                }
                else
                {
                    var result = new AuthModel.AuthResult { Message = "INVALID" };
                    return result;
                }
            }
            throw new Exception("NOT_FOUND");
        }
        public async Task Register(AuthModel.RegisterRequest request)
        {
            var customer = db.Customers.Where(x => !string.IsNullOrEmpty(x.Email) && x.Email.ToLower().Equals(request.Email.ToLower().Trim())).FirstOrDefault();
            if (customer != null)
            {
                throw new Exception("RECORD_EXISTS");
            }
            await customerService.SaveCustomer(new CustomerModel.CustomerRequest
            {
                Active = true,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Password = request.Password,
                IsTaxExempt = false,
                Username = string.IsNullOrEmpty(request.Username) ? request.Email : request.Username,
            });
        }

        private string GenerateJSONWebToken(AuthModel.JwtCustomerDetails info)
        {
            var jwtKey = configuration["Jwt:Key"];
            var jwtIssuer = configuration["Jwt:Issuer"];
            var jwtAudience = configuration["Jwt:Audience"];
            if (string.IsNullOrEmpty(jwtKey)) throw new Exception("JWT_KEY_MISSING");
            if (string.IsNullOrEmpty(jwtIssuer)) throw new Exception("JWT_ISSUER_MISSING");
            if (string.IsNullOrEmpty(jwtAudience)) throw new Exception("JWT_AUDIENCE_MISSING");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new("Id", info.Id),
        new("FirstName", info.FirstName),
        new("LastName", info.LastName),
        new(JwtRegisteredClaimNames.Email, info.Email),
                    new(JwtRegisteredClaimNames.Aud, jwtAudience),
            new(JwtRegisteredClaimNames.Iss, jwtIssuer)
                }),
                Expires = DateTime.Now.AddHours(24),
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature),
            };
            // Create token handler
            var tokenHandler = new JwtSecurityTokenHandler();

            // Generate JWT
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // Convert JWT to string
            var tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }

        private string GenerateUserJSONWebToken(AuthModel.JwtUserDetails info, bool lifeTimeExpiry)
        {
            var jwtKey = configuration["Jwt:Key"];
            var jwtIssuer = configuration["Jwt:Issuer"];
            var jwtAudience = configuration["Jwt:Audience"];
            if (string.IsNullOrEmpty(jwtKey)) throw new Exception("JWT_KEY_MISSING");
            if (string.IsNullOrEmpty(jwtIssuer)) throw new Exception("JWT_ISSUER_MISSING");
            if (string.IsNullOrEmpty(jwtAudience)) throw new Exception("JWT_AUDIENCE_MISSING");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new("Id", info.Id),
        new("Guid", info.Guid),
        new("FirstName", info.FirstName),
        new("LastName", info.LastName),
        new("Roles", JsonConvert.SerializeObject(info.Roles)),
                    new(JwtRegisteredClaimNames.Aud, jwtAudience),
            new(JwtRegisteredClaimNames.Iss, jwtIssuer)
                }),
                Expires = lifeTimeExpiry ? DateTime.Now.AddDays(365) : DateTime.Now.AddHours(24),
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature),
            };
            // Create token handler
            var tokenHandler = new JwtSecurityTokenHandler();

            // Generate JWT
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // Convert JWT to string
            var tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }

        public int GetUserId(string jwtToken)
        {
            jwtToken = jwtToken.Split(' ')[1];
            if (!IsTokenValid(jwtToken)) throw new UnauthorizedAccessException("TOKEN_NOT_FOUND");
            // Parse the JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = tokenHandler.ReadJwtToken(jwtToken);
            _ = int.TryParse(jwtSecurityToken.Claims.Where(x => x.Type == "Id").FirstOrDefault()?.Value, out int id);
            return id;
        }
        public int GetCustomerId(string jwtToken)
        {
            jwtToken = jwtToken.Split(' ')[1];
            if (!IsCustomerTokenValid(jwtToken)) throw new UnauthorizedAccessException("TOKEN_NOT_FOUND");
            // Parse the JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = tokenHandler.ReadJwtToken(jwtToken);
            _ = Guid.TryParse(jwtSecurityToken.Claims.Where(x => x.Type == "Id").FirstOrDefault()?.Value, out Guid id);
            var CustomerId = db.Customers.FirstOrDefault(x => x.Guid == id)?.Id;
            if (!CustomerId.HasValue) throw new Exception("INVALID_TOKEN");
            return CustomerId.Value;
        }
        public bool IsTokenValid(string token)
        {
            var key = $"USER_TOKEN_{token}";
            // Check if token exists in cache
            var cachedToken = cacheService.GetValue(token);
            if (cachedToken != null)
            {
                return true; // Token is valid
            }


            // Token not found in cache, check database
            var tokenFromDb = db.UserAuthTokens.FirstOrDefault(x => x.Token == token)?.Token;
            if (tokenFromDb != null)
            {
                // Cache the token with an appropriate expiration time
                cacheService.SetValue(key, tokenFromDb, 1);
                return true; // Token is valid
            }



            return false; // Token is not valid
        }

        public bool IsCustomerTokenValid(string token)
        {
            var key = $"TOKEN_{token}";
            // Check if token exists in cache
            var cachedToken = cacheService.GetValue(token);
            if (cachedToken != null)
            {
                return true; // Token is valid
            }


            // Token not found in cache, check database
            var tokenFromDb = db.CustomerAuthTokens.FirstOrDefault(x => x.Token == token)?.Token;
            if (tokenFromDb != null)
            {
                // Cache the token with an appropriate expiration time
                cacheService.SetValue(key, tokenFromDb, 1);
                return true; // Token is valid
            }



            return false; // Token is not valid
        }

        public enum UserType
        {
            User, Customer
        }
    }

}
