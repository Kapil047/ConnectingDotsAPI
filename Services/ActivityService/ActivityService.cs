using Amazon.S3.Model;
using ConnectingDotsAPI.DBModels;
using ConnectingDotsAPI.Models;


namespace ConnectingDotsAPI.Services.ActivityService
{
    public class ActivityService : IActivityService
    {
        private readonly ConnectingDotsDbContext db;

        public ActivityService(ConnectingDotsDbContext db)
        {
            this.db = db;
            ValidateActivityLogType();
        }

        public async Task LogActivity(ActivityLogModel.LogActivityRequest request)
        {
            db.ActivityLogs.Add(new ActivityLog
            {
                CreatedOn = DateTime.Now,
                Comment = request.Comment,
                ActivityLogTypeId = request.ActivityLogTypeId,
                UserId = request.UserId,
                EntityId = request.EntityId,
                EntityName = request.EntityName,
                IpAddress = request.IpAddress,
            });
            await db.SaveChangesAsync();
        }
        public int GetActivityLogType(string activityType)
        {
            if (!db.ActivityLogTypes.Any(x =>activityType.Trim().ToLower() == x.SystemKeyword.ToLower()))
                throw new Exception("ACTIVITY LOG TYPE NOT FOUND");
            return db.ActivityLogTypes.First(x => activityType.Trim().ToLower()==x.SystemKeyword.ToLower()).Id;
        }
        public async void ValidateActivityLogType()
        {
            if(!db.ActivityLogTypes.Any(x=>x.SystemKeyword=="insert"))
            {
                db.ActivityLogTypes.Add(new ActivityLogType
                {
                    SystemKeyword = "insert",
                    Enabled = true,
                    Name = "Insert"
                });
            }
            if(!db.ActivityLogTypes.Any(x=>x.SystemKeyword=="update"))
            {
                db.ActivityLogTypes.Add(new ActivityLogType
                {
                    SystemKeyword = "update",
                    Enabled = true,
                    Name = "Update"
                });
            }
            if(!db.ActivityLogTypes.Any(x=>x.SystemKeyword=="delete"))
            {
                db.ActivityLogTypes.Add(new ActivityLogType
                {
                    SystemKeyword = "delete",
                    Enabled = true,
                    Name = "Delete"
                });
            }
            await db.SaveChangesAsync();
        }
    }
}
