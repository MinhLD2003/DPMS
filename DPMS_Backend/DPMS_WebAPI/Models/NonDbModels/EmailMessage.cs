namespace DPMS_WebAPI.Models.NonDbModels
{
    public class EmailMessage
    {
        public string From { get; set; }
        public string FromDisplayName { get; set; }
        public List<EmailRecipient> To { get; set; } = new List<EmailRecipient>();
        public List<EmailRecipient> Cc { get; set; } = new List<EmailRecipient>();
        public List<EmailRecipient> Bcc { get; set; } = new List<EmailRecipient>();
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsHtml { get; set; }
        public List<EmailAttachment> Attachments { get; set; } = new List<EmailAttachment>();

        // Helper methods for easier use
        public void AddRecipient(string email, string name = null)
        {
            To.Add(new EmailRecipient { Email = email, DisplayName = name });
        }
    }

    public class EmailRecipient
    {
        public string Email { get; set; }
        public string DisplayName { get; set; }
    }

    public class EmailAttachment
    {
        public string FileName { get; set; }
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
    }
}
