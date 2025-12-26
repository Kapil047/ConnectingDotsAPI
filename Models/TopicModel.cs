namespace ConnectingDotsAPI.Models
{
    public class TopicModel
    {
        public class TopicRequest
        {
            public int? Id { get; set; }
            public string? SystemName { get; set; }
            public bool IncludeInSitemap { get; set; }
            public bool IncludeInTopMenu { get; set; }
            public bool IncludeInFooterColumn1 { get; set; }
            public bool IncludeInFooterColumn2 { get; set; }
            public bool IncludeInFooterColumn3 { get; set; }
            public int DisplayOrder { get; set; }

            public bool IsPasswordProtected { get; set; } = false;
            public string? Password { get; set; }
            public string? Title { get; set; }
            public string? Body { get; set; }
            public bool Published { get; set; }
            public int TopicTemplateId { get; set; }
            public string? MetaKeywords { get; set; }
            public string? MetaDescription { get; set; }
            public string? MetaTitle { get; set; }

            public string? Slug { get; set; }
        }
        public class TopicDetails
        {
            public int Id { get; set; }
            public string? SystemName { get; set; }
            public bool IncludeInSitemap { get; set; }
            public bool IncludeInTopMenu { get; set; }
            public bool IncludeInFooterColumn1 { get; set; }
            public bool IncludeInFooterColumn2 { get; set; }
            public bool IncludeInFooterColumn3 { get; set; }
            public int DisplayOrder { get; set; }

            public bool IsPasswordProtected { get; set; }
            public string? Password { get; set; }
            public string? Title { get; set; }
            public string? Body { get; set; }
            public bool Published { get; set; }
            public int TopicTemplateId { get; set; }
            public string? MetaKeywords { get; set; }
            public string? MetaDescription { get; set; }
            public string? MetaTitle { get; set; }

            public string? Slug { get; set; }
        }
    }
}
