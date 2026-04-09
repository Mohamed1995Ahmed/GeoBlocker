using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTO
{
	public class BlockCountryRequest
	{
		/// <summary>ISO 3166-1 alpha-2 country code (e.g. "EG", "US")</summary>
		public string CountryCode { get; set; } = string.Empty;
	}
}
