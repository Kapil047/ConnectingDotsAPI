using ConnectingDotsAPI.DBModels;

namespace ConnectingDotsAPI.Services.ErrorLoggingService
{
    public class ErrorLoggingService(ConnectingDotsDbContext db):IErrorLoggingService
    {
        private readonly ConnectingDotsDbContext db = db;

        public async Task LogError(string exception, string innerException, string pageUrl, int? customerId, int logLevelId = 10)
        {
            db.Logs.Add(new Log
            {
                CreatedOnUtc = DateTime.UtcNow,
                CustomerId = customerId,
                FullMessage = innerException,
                ShortMessage = exception,
                LogLevelId = logLevelId,
                PageUrl = pageUrl,
            });
            await db.SaveChangesAsync();
        }
    }
}
