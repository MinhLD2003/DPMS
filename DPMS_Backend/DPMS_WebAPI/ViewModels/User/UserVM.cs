namespace DPMS_WebAPI.ViewModels
{
    public class UserVM
    {
        public Guid Id { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
    }
}