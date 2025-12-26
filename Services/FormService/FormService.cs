using ConnectingDotsAPI.DBModels;
using ConnectingDotsAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ConnectingDotsAPI.Services.FormService
{
    public class FormService(ConnectingDotsDbContext db) : IFormService
    {
        private readonly ConnectingDotsDbContext db = db;

        public async Task<Form> SaveForm(FormModel.FormRequest request)
        {
            var val = new Form() { Guid = Guid.NewGuid(), Deleted = false, Active = true };

            if (!string.IsNullOrEmpty(request.Guid))
                val = db.Forms.FirstOrDefault(x => x.Guid == Guid.Parse(request.Guid));
            if (val == null) throw new Exception();
            val.Name = request.Name;
            val.Description = request.Description;
            val.Active = request.Active;
            val.DisplayOrder = request.DisplayOrder;
            if (val.Id == 0)
            {
                db.Forms.Add(val);
            }
            await db.SaveChangesAsync();
            return new Form() { Id = val.Id, Guid = val.Guid };
        }
        public async Task<List<FormModel.FormDetails>> GetAll()
        {
            return await db.Forms.Where(x => !x.Deleted)

                      .Select(value => new FormModel.FormDetails
                      {
                          Guid = value.Guid,
                          Name = value.Name,
                          Description = value.Description,
                          Active = value.Active,
                          DisplayOrder = value.DisplayOrder,
                      }).OrderBy(x=>x.DisplayOrder).ToListAsync();
        }
        public FormModel.FormDetails? GetDetails(Guid? Guid, int? Id)
        {
            return db.Forms.Where(x => x.Guid == Guid || x.Id == Id)
                .Include(x => x.FormResponses).ThenInclude(x => x.Form)
                .Include(x => x.Questions).ThenInclude(x => x.Options)
                     .Select(value => new FormModel.FormDetails
                     {
                         Guid = value.Guid,
                         Name = value.Name,
                         Description = value.Description,
                         Active = value.Active,
                         DisplayOrder= value.DisplayOrder,
                         FormResponses = value.FormResponses.Select(x => new
                         {

                             x.Id,
                             CustomerGuid = x.Customer.Guid,
                             x.Customer.FirstName,
                             x.Customer.LastName,
                             x.SubmittedAt,
                             x.Remarks,
                             Response = db.QuestionResponses.Where(q => q.FormResponseId == x.Id).Select(q => new { q.Id, q.Response }).ToList(),
                         }),
                         Questions = value.Questions.Where(x => !x.Deleted).Select(x => new
                         {
                             x.Text,
                             x.Guid,
                             x.ControlType,
                             x.Active,
                             Options = x.Options.Select(x => new { x.Id, x.Text })
                         })
                     }).FirstOrDefault();

        }
        public async Task<Form> Delete(Guid guid)
        {
            var val = db.Forms.FirstOrDefault(x => x.Guid == guid)
                ?? throw new Exception("NOT_FOUND");
            val.Deleted = true;
            await db.SaveChangesAsync();
            return new Form { Id = val.Id, Guid = val.Guid };
        }

        #region  Question
        public async Task<List<FormModel.QuestionDetails>> GetAllQuestions()
        {
            return await db.Questions.Where(x => !x.Deleted)

                      .Select(value => new FormModel.QuestionDetails
                      {
                          Guid = value.Guid,
                          Text = value.Text,
                          Active = value.Active,
                          ControlType = value.ControlType,
                          DisplayOrder = value.DisplayOrder,
                      }).OrderBy(x=>x.DisplayOrder).ToListAsync();
        }
        public async Task<Question> SaveQuestion(FormModel.QuestionRequest request)
        {
            var question = new Question { Active = true, Deleted = false, Guid = Guid.NewGuid() };
            if (!string.IsNullOrEmpty(request.Guid))
                question = db.Questions.FirstOrDefault(x => x.Guid == Guid.Parse(request.Guid)) ?? throw new Exception("NOT FOUND");
            question.DisplayOrder = request.DisplayOrder;
            question.Active = request.Active;
            question.ControlType = request.ControlType;
            question.Text = request.Text;
            if (question.Id == 0)
                db.Questions.Add(question);
            await db.SaveChangesAsync();
            return new Question { Id = question.Id, Guid = question.Guid };
        }
        public FormModel.QuestionDetails? GetQuestionDetails(Guid? Guid, int? Id)
        {
            return db.Questions.Where(x => x.Guid == Guid || x.Id == Id)
               .Include(x => x.QuestionResponses)
               .Include(x => x.Options)

                     .Select(value => new FormModel.QuestionDetails
                     {
                         Guid = value.Guid,
                         Text = value.Text,
                         ControlType = value.ControlType,
                         Active = value.Active,
                         DisplayOrder = value.DisplayOrder,
                         Options = value.Options.Select(x => new { x.Id, x.Text }),
                         QuestionResponses = value.QuestionResponses.Select(x => new
                         {
                             x.Response,
                             x.Question.Text,
                             x.Question.Guid,
                         }),
                     }).FirstOrDefault();

        }
        public async Task<Question> DeleteQuestion(Guid questionGuid)
        {
            var val = db.Questions.FirstOrDefault(x => x.Guid == questionGuid) ?? throw new Exception("NOT FOUND");
            val.Deleted = true;
            await db.SaveChangesAsync();
            return new Question { Id = val.Id };
        }
        public async Task<FormResponse> DeleteResponse(int formResponseId)
        {
            var val = db.FormResponses.FirstOrDefault(x => x.Id == formResponseId) ?? throw new Exception("NOT FOUND");

            db.QuestionResponses.RemoveRange(db.QuestionResponses.Where(x => x.FormResponseId == formResponseId));
            await db.SaveChangesAsync();
            db.FormResponses.Remove(val);
            await db.SaveChangesAsync();


            return new FormResponse { Id = val.Id };
        }
        public async Task SubmitAnswer(FormModel.FormResponseRequest request, int customerId)
        {
            var form = db.Forms.FirstOrDefault(x => x.Guid == Guid.Parse(request.FormGuid)) ?? throw new Exception("FORM NOT FOUND");


            foreach (var item in request.Response)
            {
                var _response = new FormResponse
                {
                    CustomerId = customerId,
                    FormId = form.Id,
                    SubmittedAt = DateTime.Now,
                    Remarks = item.ApplicantType
                };
                db.FormResponses.Add(_response);

                await db.SaveChangesAsync();

                foreach (var submission in item.Submissions)
                {
                    var question = db.Questions.FirstOrDefault(x => x.Guid == Guid.Parse(submission.QuestionGuid)) ?? throw new Exception("QUESTION NOT FOUND");

                    db.QuestionResponses.Add(new QuestionResponse
                    {
                        FormResponseId = _response.Id,
                        QuestionId = question.Id,
                        Response = submission.SelectedValue,

                    });
                }
                await db.SaveChangesAsync();

            }

        }
        #endregion

        #region  Option
        public async Task<Option> DeleteOption(int optionId)
        {
            var option = db.Options.FirstOrDefault(x => x.Id == optionId) ?? throw new Exception("NOT FOUND");
            db.Options.Remove(option);
            await db.SaveChangesAsync();
            return new Option { Id = option.Id };
        }
        public async Task<Option> AddOption(FormModel.OptionRequest request)
        {
            var question = db.Questions.FirstOrDefault(x => x.Guid == Guid.Parse(request.QuestionGuid)) ?? throw new Exception("NOT FOUND");
            var option = new Option
            {
                QuestionId = question.Id,
                Text = request.Text,
            };
            db.Options.Add(option);
            await db.SaveChangesAsync();
            return new Option { Id = option.Id };
        }
        #endregion

        public async Task<(int formId, int questionId)> UpdateFormQuestionMapping(FormModel.FormQuestionMappingRequest request)
        {
            var form = db.Forms.Where(u => u.Guid == Guid.Parse(request.FormGuid)).Include(u => u.Questions).FirstOrDefault() ?? throw new Exception("QUESTION NOT_FOUND");
            var question = db.Questions.FirstOrDefault(r => r.Guid == Guid.Parse(request.QuestionGuid)) ?? throw new Exception("QUESTION_NOT_FOUND");
            if (request.Action == UserModel.MethodType.delete)
            {
                form.Questions.Remove(question);
            }
            else
            {
                if (!form.Questions.Any(u => u.Id == form.Id))
                    form.Questions.Add(question);
                await db.SaveChangesAsync();
            }
            //sending form id as the main entity for logs is student here
            return (form.Id, question.Id);
        }
    }
}
