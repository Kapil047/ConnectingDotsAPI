
namespace ConnectingDotsAPI.Services.ErrorLoggingService
{
    public interface IErrorLoggingService
    {
        Task LogError(string exception, string innerException, string pageUrl, int? customerId, int logLevelId = 10);
    }
}