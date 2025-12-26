using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class Question
{
    public int Id { get; set; }

    public Guid Guid { get; set; }

    public string Text { get; set; } = null!;

    public string ControlType { get; set; } = null!;

    public bool Active { get; set; }

    public bool Deleted { get; set; }

    public int DisplayOrder { get; set; }

    public virtual ICollection<Option> Options { get; set; } = new List<Option>();

    public virtual ICollection<QuestionResponse> QuestionResponses { get; set; } = new List<QuestionResponse>();

    public virtual ICollection<Form> Forms { get; set; } = new List<Form>();
}
