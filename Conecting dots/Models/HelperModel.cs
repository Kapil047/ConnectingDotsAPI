namespace ConnectingDotsAPI.Models
{
    public class HelperModel
    {
        public class PaginatedList<T>
        {
            public required List<T> Data { get; set; }
            public int RecordsTotal{ get; set; }
            public int RecordsFiltered { get; set; }
            public int Draw { get; set; }

           
        }
    }
}
