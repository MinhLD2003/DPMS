namespace DPMS_WebAPI.ViewModels.Form
{
    public class ExportFICSubmissionVM
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public Guid FormId { get; set; }
        public string Name { get; set; }
        public int OrderIndex { get; set; }
        public int HierarchyLevel { get; set; }
        public string SoftPath { get; set; }
        public Guid? SubmissionId { get; set; }
        public string? FormElementValue { get; set; }
    }
}
