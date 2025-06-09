namespace DPMS_WebAPI.ViewModels
{
    #pragma warning disable CS1591
    public class QueryParams
    {
        private const int MaxPageSize = 100;
        private int _pageSize = 10;
        public int PageNumber { get; set; } = 1;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
        public string SortBy { get; set; } = string.Empty;
        public string SortDirection { get; set; } = "asc";
        public Dictionary<string, string> Filters { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}   