using ConnectingDotsAPI.DBModels;
using ConnectingDotsAPI.Models;

namespace ConnectingDotsAPI.Services.FormService
{
    public interface IFormService
    {
        Task<List<FormModel.QuestionDetails>> GetAllQuestions();
        Task<Question> SaveQuestion(FormModel.QuestionRequest request);
        FormModel.QuestionDetails? GetQuestionDetails(Guid? Guid, int? Id);
        Task<Question> DeleteQuestion(Guid questionGuid);
        Task<Option> AddOption(FormModel.OptionRequest request);
        Task<Form> Delete(Guid guid);
        Task<Option> DeleteOption(int optionId);
        Task<List<FormModel.FormDetails>> GetAll();
        FormModel.FormDetails? GetDetails(Guid? Guid, int? Id);
        Task<Form> SaveForm(FormModel.FormRequest request);
        Task<(int formId, int questionId)> UpdateFormQuestionMapping(FormModel.FormQuestionMappingRequest request);
        Task SubmitAnswer(FormModel.FormResponseRequest request, int customerId);
        Task<FormResponse> DeleteResponse(int formResponseId);
    }
}