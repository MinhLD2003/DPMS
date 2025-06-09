

using System.Text.Json.Serialization;

namespace DPMS_WebAPI.Models
{
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    
    public abstract class BaseModel
    {
        public BaseModel()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            LastModifiedAt = DateTime.UtcNow;
        }
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastModifiedAt { get; set; }
        // navigation properties
        // Store only GUIDs, no navigation properties
        public Guid? CreatedById { get; set; }
        public Guid? LastModifiedById { get; set; }

        // Navigation properties
        [JsonIgnore] // Prevent circular reference in API responses
        public virtual User? CreatedBy { get; set; }

        [JsonIgnore]
        public virtual User? LastModifiedBy { get; set; }
    }
}