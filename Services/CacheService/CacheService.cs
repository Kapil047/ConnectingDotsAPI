using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Caching.Memory;

namespace ConnectingDotsAPI.Services.CacheService
{
    public class CacheService(IMemoryCache cache, IHttpContextAccessor httpContextAccessor) : ICacheService
    {
        private readonly IMemoryCache cache = cache;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        private string GetClientPrefixedKey(string key)
        {
              var url = _httpContextAccessor.HttpContext.Request.Host;
            return $"{url}_{key}";
        }


        public string? GetValue(string Key)
        {
            var clientKey = GetClientPrefixedKey(Key);
            var value = cache.Get(clientKey);
            return value?.ToString();
        }
        public bool SetValue(string Key, string Value, double? expiryMinutes = 24 * 60 * 60)
        {
            var clientKey = GetClientPrefixedKey(Key);
            var dateTime = DateTime.Now + (expiryMinutes.HasValue ? TimeSpan.FromMinutes(expiryMinutes.Value) : new TimeSpan());
            dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            DateTimeOffset utcTime2 = dateTime;
            cache.Set(clientKey, Value, utcTime2);
            return true;
        }
        public void Delete(string Key)
        {
            var clientKey = GetClientPrefixedKey(Key);
            cache.Remove(clientKey);
        }
    }
}
