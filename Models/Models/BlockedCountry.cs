using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Models
{
	public class BlockedCountry
	{
		public string CountryCode { get; set; } = string.Empty;   // e.g. "EG"
		public string CountryName { get; set; } = string.Empty;   // e.g. "Egypt"
		public DateTime BlockedAt { get; set; } = DateTime.UtcNow;
	}
}
