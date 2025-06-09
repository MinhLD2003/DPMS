namespace DPMS_WebAPI.ViewModels
{
    public class PagedResponse<T>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public List<T> Data { get; set; } = new List<T>();
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;
        
        public PagedResponse()
        {
            Data = new List<T>();
        }
    }
}