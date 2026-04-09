using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Models
{
	public class TemporalBlock
	{
		public string CountryCode { get; set; } = string.Empty;
		public string CountryName { get; set; } = string.Empty;
		public DateTime BlockedAt { get; set; } = DateTime.UtcNow;
		public DateTime ExpiresAt { get; set; }
		public int DurationMinutes { get; set; }
	}
}
