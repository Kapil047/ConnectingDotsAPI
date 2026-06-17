using ConnectingDotsAPI.DBModels;
using ConnectingDotsAPI.Services.CacheService;
using Microsoft.Extensions.Caching.Memory;

namespace ConnectingDotsAPI.Services
{
    public class TokenValidationMiddleware(IMemoryCache cacheService, RequestDelegate next)
    {
        private readonly IMemoryCache cacheService = cacheService;
        private readonly RequestDelegate _next =next;

        public async Task InvokeAsync(HttpContext context)
        {

            // Check if the request has a token in the headers
            if (context.Request.Headers.TryGetValue("Authorization", out var authHeaderValue))
            {
                var token = authHeaderValue.ToString().Split(' ')[1];
                // Validate the token
                if (IsTokenValid(token))
                {
                    // Token is valid, continue with the request pipeline
                    await _next(context);
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                }
            }
            else
            {
                await _next(context);
            }
            // Token is invalid or missing, return unauthorized
            //context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }


        public bool IsTokenValid(string token)
        {
            // Check if token exists in cache
            var cachedToken = cacheService.Get(token);
            if (cachedToken != null)
            {
                return true; // Token is valid
            }

            // Token not found in cache, check database
            using ConnectingDotsDbContext db = new();
            var tokenFromDb = db.UserAuthTokens.FirstOrDefault(x => x.Token == token)?.Token;
            if (tokenFromDb != null)
            {
                // Cache the token with an appropriate expiration time
                cacheService.Set(token, tokenFromDb, TimeSpan.FromMinutes(30));
                return true; // Token is valid
            }

            var tokenFromCustomerDb = db.CustomerAuthTokens.FirstOrDefault(x => x.Token == token)?.Token;
            if (tokenFromCustomerDb != null)
            {
                // Cache the token with an appropriate expiration time
                cacheService.Set(token, tokenFromDb, TimeSpan.FromMinutes(30));
                return true; // Token is valid
            }
            return false; // Token is not valid
        }
    }

}
