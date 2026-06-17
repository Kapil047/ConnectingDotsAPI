namespace ConnectingDotsAPI.Services.CacheService
{
    public interface ICacheService
    {
        void Delete(string Key);
        string? GetValue(string Key);
        bool SetValue(string Key, string Value, double? expiryMinutes=24*60*60);
    }
}