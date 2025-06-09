namespace DPMS_WebAPI.Models
{
	#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

	public class HttpLog
	{
		public Guid Id { get; set; }
		public string? TraceId { get; set; }
		public int? HttpType { get; set; } // HTTP Request = 1, HTTP Response = 2
		public int? ResponseStatus { get; set; } // Status code for HTTP Response
		public string? Email { get; set; }
		public string? Method { get; set; }
		public string? IpAddress { get; set; }
		public string? Url { get; set; }
		public string? UserAgent { get; set; }
		public DateTime AccessedTime { get; set; } = DateTime.UtcNow;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}
