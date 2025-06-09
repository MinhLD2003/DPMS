using Microsoft.EntityFrameworkCore;

namespace DPMS_WebAPI.Models
{
	#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

	public class DPMSLoggingContext : DbContext
	{
		public DPMSLoggingContext(DbContextOptions<DPMSLoggingContext> options) : base(options) { }

		public DbSet<HttpLog> HttpLogs { get; set; }
	}
}
