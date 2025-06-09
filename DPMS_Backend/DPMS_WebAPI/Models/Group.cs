using DPMS_WebAPI.Enums;

namespace DPMS_WebAPI.Models
{
	#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

	public class Group : BaseModel
	{
		public required string Name { get; set; }
		public string? Description { get; set; }
		public bool IsGlobal { get; set; }  // true nếu đây là badge toàn cục, false nếu là badge cụ thể của một hệ thống

		// navigational properties
		public Guid? SystemId { get; set; } // Null nếu group là badge toàn cụcs
		public ExternalSystem? System { get; set; } // Nếu group là badge toàn cục thì System sẽ null
		public List<User>? Users { get; set; } 
		public ICollection<Feature> Features { get; set; } = new List<Feature>(); // Một group có thể có nhiều feature
		public List<UserGroup> UserGroups { get; set; } = new List<UserGroup>(); // Một group có thể có nhiều user
		public ICollection<GroupFeature> GroupFeatures { get; set;  } = new List<GroupFeature>();
	}
}
