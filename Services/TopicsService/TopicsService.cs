using Azure.Core;
using ConnectingDotsAPI.DBModels;
using ConnectingDotsAPI.Models;
using ConnectingDotsAPI.Services.HelperService;
using Microsoft.EntityFrameworkCore;

namespace ConnectingDotsAPI.Services.TopicsService
{
    public class TopicsService(ConnectingDotsDbContext db, IHelperService helperService) : ITopicsService
    {
        private readonly IHelperService helperService = helperService;
        private readonly ConnectingDotsDbContext db = db;
        #region Topics
        public async Task DeleteTopic(int id)
        {
            var topic = db.Topics.FirstOrDefault(t => t.Id == id) ?? throw new Exception("NOT_FOUND");
            topic.Deleted = true;
            await db.SaveChangesAsync();

            await helperService.DeactivateUrlRecord(topic.Id, "Topic");

        }
        public async Task<int> SaveTopic(TopicModel.TopicRequest request)
        {
            var topic = db.Topics.FirstOrDefault(t => t.Id == request.Id) ?? new Topic();
            topic.IncludeInFooterColumn1 = request.IncludeInFooterColumn1;
            topic.IncludeInFooterColumn2 = request.IncludeInFooterColumn2;
            topic.IncludeInFooterColumn3 = request.IncludeInFooterColumn3;
            topic.IncludeInSitemap = request.IncludeInSitemap;
            topic.IncludeInTopMenu = request.IncludeInTopMenu;
            topic.MetaDescription = request.MetaDescription;
            topic.MetaTitle = request.MetaTitle;
            topic.MetaKeywords = request.MetaKeywords;
            topic.Published = request.Published;
            topic.SystemName = request.SystemName;
            topic.Body = request.Body;
            topic.DisplayOrder = request.DisplayOrder;
            topic.Title = request.Title;
            topic.TopicTemplateId = 1;
            if (topic.Id == 0)
                db.Topics.Add(topic);
            await db.SaveChangesAsync();
            if (!string.IsNullOrEmpty(request.Slug))
                await helperService.UpdateUrlRecord(topic.Id, "Topic", request.Slug);

            return topic.Id;
        }
        public async Task<List<TopicModel.TopicDetails>> GetTopics()
        {
            return await db.Topics.Where(x=>!x.Deleted).Select(t => new TopicModel.TopicDetails
            {
                IncludeInFooterColumn1 = t.IncludeInFooterColumn1,
                IncludeInFooterColumn2 = t.IncludeInFooterColumn2,
                IncludeInFooterColumn3 = t.IncludeInFooterColumn3,
                Published = t.Published,
                DisplayOrder = t.DisplayOrder,
                IncludeInTopMenu = t.IncludeInTopMenu,
                SystemName = t.SystemName,
                Id = t.Id,
                Title = t.Title,
                Slug = db.UrlRecords
                                .Where(x =>
                                !string.IsNullOrEmpty(x.EntityName)
                                && x.EntityId == t.Id && x.EntityName == "Topic" && x.IsActive).OrderByDescending(x => x.Id).First().Slug
            }).ToListAsync();
        }
        public TopicModel.TopicDetails GetTopicDetails(int id)
        {
            return db.Topics.Where(T => T.Id == id).Select(t => new TopicModel.TopicDetails
            {
                IncludeInFooterColumn1 = t.IncludeInFooterColumn1,
                IncludeInFooterColumn2 = t.IncludeInFooterColumn2,
                IncludeInFooterColumn3 = t.IncludeInFooterColumn3,
                Published = t.Published,
                DisplayOrder = t.DisplayOrder,
                IncludeInTopMenu = t.IncludeInTopMenu,
                SystemName = t.SystemName,
                MetaDescription = t.MetaDescription,
                MetaKeywords = t.MetaKeywords,
                TopicTemplateId = t.TopicTemplateId,
                Body = t.Body,
                IsPasswordProtected = t.IsPasswordProtected,
                MetaTitle = t.MetaTitle,
                Slug = db.UrlRecords
                                .Where(x =>
                                !string.IsNullOrEmpty(x.EntityName)
                                && x.EntityId == t.Id && x.EntityName == "Topic" && x.IsActive).OrderByDescending(x => x.Id).First().Slug,
                Id = t.Id,
                Title = t.Title,

            }).First();
        }
        #endregion
    }
}
