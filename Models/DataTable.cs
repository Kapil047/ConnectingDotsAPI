namespace ConnectingDotsAPI.Models
{
    public class DataTablesParameters
    {
        public int Draw { get; set; }
        public DTColumn[] Columns { get; set; }
        public DTOrder[] Order { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public DTSearch Search { get; set; }
        public string? SortOrder { get; }
        public IEnumerable<string>? AdditionalValues { get; set; }

        public class DTColumn
        {
            public string Data { get; set; }
            public string Name { get; set; }
            public bool Searchable { get; set; }
            public bool Orderable { get; set; }
            public DTSearch Search { get; set; }
        }
        public class DTOrder
        {
            public int Column { get; set; }
            public string Dir { get; set; }
        }
        public class DTSearch
        {
            public string Value { get; set; }
            public bool Regex { get; set; }
        }
        public enum DTOrderDir
        {
            ASC = 0,
            DESC = 1
        }
    }
}
