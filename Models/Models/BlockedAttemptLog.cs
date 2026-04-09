using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Models
{
	public class BlockedAttemptLog
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public string IpAddress { get; set; } = string.Empty;
		public DateTime Timestamp { get; set; } = DateTime.UtcNow;
		public string CountryCode { get; set; } = string.Empty;
		public string CountryName { get; set; } = string.Empty;
		public bool IsBlocked { get; set; }
		public string UserAgent { get; set; } = string.Empty;
	}
}
