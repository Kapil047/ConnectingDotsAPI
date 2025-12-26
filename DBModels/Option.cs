using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class Option
{
    public int Id { get; set; }

    public int QuestionId { get; set; }

    public string Text { get; set; } = null!;

    public virtual Question Question { get; set; } = null!;
}
