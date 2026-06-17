using ConnectingDotsAPI.DBModels;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace ConnectingDotsAPI.Models
{
    public class FormModel
    {
        public class FormRequest
        {
            [Required]
            [MaxLength(255)]
            public required string Name { get; set; }
            public string? Guid { get; set; }
            public string? Description { get; set; }
            public bool Active { get; set; }
            public int DisplayOrder { get; set; } = 0;
        }
        public class FormDetails
        {
            public required string Name { get; set; }
            public Guid Guid { get; set; }
            public string? Description { get; set; }
            public bool Active { get; set; }
            public object? FormResponses { get; set; }
            public object? Questions { get; set; }
            public int DisplayOrder { get; set; }
        }

        public class QuestionDetails
        {
            public Guid Guid { get; set; }
            public required string Text { get; set; }
            public required string ControlType { get; set; }
            public bool Active { get; set; }
            public object? Options { get; set; }
            public object? QuestionResponses { get; set; }
            public int DisplayOrder { get; set; }

        }

        public class QuestionRequest
        {
            public string? Guid { get; set; }
            [Required]
            [MaxLength(255)]
            public required string Text { get; set; }
            [Required]
            [MaxLength(50)]
            public required string ControlType { get; set; }
            public bool Active { get; set; }
            public int DisplayOrder { get; set; } = 0;
        }

        public class OptionRequest
        {
            [Required]
            public required string QuestionGuid { get; set; }
            [Required]
            [MaxLength(255)]
            public required string Text { get; set; }
        }

        public class FormQuestionMappingRequest
        {
            [Required]
            public required string QuestionGuid { get; set; }
            [Required]
            public required string FormGuid { get; set; }
            [Required]
            public UserModel.MethodType Action { get; set; }
        }

        public class FormResponseRequest
        {
            [Required]
            public required string FormGuid { get; set; }
            [Required]
            public required List<Applicant> Response { get; set; }
        }
        public class Applicant
        {
            public required string ApplicantType { get; set; }
            public required List<Submission> Submissions { get; set; }
        }
        public class Submission
        {
            public required string QuestionGuid { get; set; }
            public required string SelectedValue { get; set; }
        }


    }
}
