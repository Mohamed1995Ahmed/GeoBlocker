using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Models
{
	public class IpLookupResult
	{
		public string IpAddress { get; set; } = string.Empty;
		public string CountryCode { get; set; } = string.Empty;
		public string CountryName { get; set; } = string.Empty;
		public string City { get; set; } = string.Empty;
		public string Region { get; set; } = string.Empty;
		public string Isp { get; set; } = string.Empty;
		public string Timezone { get; set; } = string.Empty;
	}
}
