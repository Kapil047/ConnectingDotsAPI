using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class FormResponse
{
    public int Id { get; set; }

    public int FormId { get; set; }

    public DateTime SubmittedAt { get; set; }

    public int CustomerId { get; set; }

    public string? Remarks { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Form Form { get; set; } = null!;

    public virtual ICollection<QuestionResponse> QuestionResponses { get; set; } = new List<QuestionResponse>();
}
