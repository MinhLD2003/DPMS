namespace DPMS_WebAPI.ViewModels.Group
{
    public class GroupUserVM
    {
        public Guid GroupId { get; set; }
        public List<Guid> UserIds { get; set; }
    }
}