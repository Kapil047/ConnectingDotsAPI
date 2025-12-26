using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class QuestionResponse
{
    public int Id { get; set; }

    public int FormResponseId { get; set; }

    public int QuestionId { get; set; }

    public string Response { get; set; } = null!;

    public virtual FormResponse FormResponse { get; set; } = null!;

    public virtual Question Question { get; set; } = null!;
}
