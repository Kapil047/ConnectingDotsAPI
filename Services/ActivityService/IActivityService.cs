using ConnectingDotsAPI.Models;

namespace ConnectingDotsAPI.Services.ActivityService
{
    public interface IActivityService
    {
        int GetActivityLogType(string activityType);
        Task LogActivity(ActivityLogModel.LogActivityRequest request);
        void ValidateActivityLogType();
    }
}