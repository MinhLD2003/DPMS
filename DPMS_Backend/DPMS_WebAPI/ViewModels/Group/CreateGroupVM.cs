namespace DPMS_WebAPI.ViewModels
{
    public class CreateGroupVM
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public List<Guid> FeatureIds { get; set; } = new List<Guid>();
        public List<Guid> UserIds { get; set; } = new List<Guid>();
    }
}