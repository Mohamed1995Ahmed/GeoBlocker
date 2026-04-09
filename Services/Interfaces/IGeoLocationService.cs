using Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
	public interface IGeoLocationService
	{
		Task<IpLookupResult?> LookupAsync(string ipAddress);
		bool IsValidIpFormat(string ipAddress);
	}
}
