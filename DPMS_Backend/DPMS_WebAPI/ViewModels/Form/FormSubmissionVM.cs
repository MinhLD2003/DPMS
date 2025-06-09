namespace DPMS_WebAPI.ViewModels.Form
{
    public class FormSubmissionVM
    {
        public Guid FormId { get; set; } // The form being submitted
        public Guid? SystemId { get; set; } // The system submitting the form
        public List<FormResponseVM> Responses { get; set; } = new();
    }

    public class FormResponseVM
    {
        public Guid FormElementId { get; set; } // The element being responded to
        public string Value { get; set; } // The response value
    }

}