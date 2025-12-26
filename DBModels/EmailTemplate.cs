using System;
using System.Collections.Generic;

namespace ConnectingDotsAPI.DBModels;

public partial class EmailTemplate
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Subject { get; set; }

    public string? SendTo { get; set; }

    public string SystemName { get; set; } = null!;

    public string TemplateHtml { get; set; } = null!;

    public bool Active { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime DateChanged { get; set; }

    public bool PreviewFirst { get; set; }
}
