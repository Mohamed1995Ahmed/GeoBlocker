using Models.DTO;
using Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
	public interface ILogService
	{
		void LogAttempt(string ipAddress, string countryCode, string countryName, bool isBlocked, string userAgent);
		PagedResponse<BlockedAttemptLog> GetLogs(int page, int pageSize);
	}
}
