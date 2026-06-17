using ConnectingDotsAPI.Models;

namespace ConnectingDotsAPI.Services.TopicsService
{
    public interface ITopicsService
    {
        Task DeleteTopic(int id);
        TopicModel.TopicDetails GetTopicDetails(int id);
        Task<List<TopicModel.TopicDetails>> GetTopics();
        Task<int> SaveTopic(TopicModel.TopicRequest request);
    }
}