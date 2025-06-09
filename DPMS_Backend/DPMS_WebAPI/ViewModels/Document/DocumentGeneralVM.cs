namespace DPMS_WebAPI.ViewModels.Document
{
    public class DocumentGeneralVM
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string FileUrl { get; set; }
        public string FileFormat { get; set; }
    }
}