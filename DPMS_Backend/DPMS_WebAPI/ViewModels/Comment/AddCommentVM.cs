using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.ViewModels.Comment
{
    public class AddCommentVM
    {
        public Guid ReferenceId { get; set; }
        public Guid UserId { get; set; }
        public string Content { get; set; }
        public CommentType Type { get; set; }
    }

    public class CommentVM
    {
        public Guid Id { get; set; }
        public Guid ReferenceId { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Content { get; set; }
        public CommentType Type { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        
    }
}