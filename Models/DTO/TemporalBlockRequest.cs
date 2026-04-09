using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTO
{
	public class TemporalBlockRequest
	{
		/// <summary>ISO 3166-1 alpha-2 country code</summary>
		public string CountryCode { get; set; } = string.Empty;

		/// <summary>Duration in minutes (1 – 1440)</summary>
		public int DurationMinutes { get; set; }
	}
}
