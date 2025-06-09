namespace DPMS_WebAPI.ViewModels.Form
{
	public class FICSubmissionVM
	{
		public Guid SystemId { get; set; }
		public Guid FormId { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
		public string ExternalSystemName { get; set; }
		public DateTime CreatedAt { get; set; }
		public string CreatedBy { get; set; }
	}
}
