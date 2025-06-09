
using System.Text.Json.Serialization;
using DPMS_WebAPI.Enums;

namespace DPMS_WebAPI.Models
{
	#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

	public class Feature : BaseModel
	{
		// Screen or ui
		public required string FeatureName { get; set; }
		public string? Description { get; set; }

		public Guid? ParentId { get; set; }
		public FeatureState State { get; set; }
		public string? Url { get; set; }
		public HttpMethodType? HttpMethod { get; set; }
		
		[JsonIgnore] 
		public Feature? Parent { get; set; }
        public ICollection<Feature> Children { get; set; } = new List<Feature>();
        public ICollection<GroupFeature> GroupFeatures { get; set; } = new List<GroupFeature>();
		public ICollection<Group> Groups { get; set; } = new List<Group>();
	}

	public enum HttpMethodType
	{
		GET,
		POST,
		PUT,
		DELETE,
		PATCH
	}

}
