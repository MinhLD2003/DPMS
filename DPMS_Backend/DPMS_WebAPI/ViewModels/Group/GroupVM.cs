namespace DPMS_WebAPI.ViewModels
{
    public class GroupVM
    {
        public Guid? Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool IsGlobal { get; set; }
    }
}