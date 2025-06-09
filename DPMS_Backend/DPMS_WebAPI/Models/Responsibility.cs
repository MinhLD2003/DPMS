namespace DPMS_WebAPI.Models
{
    public class Responsibility : BaseModel
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}