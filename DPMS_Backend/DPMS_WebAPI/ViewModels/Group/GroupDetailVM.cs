namespace DPMS_WebAPI.ViewModels
{
    public class GroupDetailVM
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool IsGlobal { get; set; }
        public SystemVM? System { get; set; }
        public List<FeatureVM> Features { get; set; } = new List<FeatureVM>();
        public List<UserVM> Users { get; set; } = new List<UserVM>();
    }

    public class SystemVM
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}