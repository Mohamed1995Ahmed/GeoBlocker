using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTO
{
	// ipgeolocation.io field names differ from ipapi.co
	public class IpapiResponse
	{
		public string? ip { get; set; }
		public string? country_code2 { get; set; }  // was country_code
		public string? country_name { get; set; }
		public string? city { get; set; }
		public string? state_prov { get; set; }  // was region
		public string? isp { get; set; }
		public string? org { get; set; }
		public TimeZoneInfo? time_zone { get; set; } // nested object
		public string? error { get; set; }
		public string? reason { get; set; }
	}

	public class TimeZoneInfo
	{
		public string? name { get; set; }
	}
}
