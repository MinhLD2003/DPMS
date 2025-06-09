namespace DPMS_WebAPI.ViewModels.DPIA
{
    public class DPIAUserVM
    {
        public Guid Id { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public List<string> GroupNames { get; set; }
    }
}
