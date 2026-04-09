using Models.DTO;
using Models.Models;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Services
{
	public class LogService : ILogService
	{
		private readonly IInMemoryRepository _repo;

		public LogService(IInMemoryRepository repo) => _repo = repo;

		public void LogAttempt(
			string ipAddress, string countryCode, string countryName,
			bool isBlocked, string userAgent)
		{
			_repo.AddLog(new BlockedAttemptLog
			{
				IpAddress = ipAddress,
				CountryCode = countryCode,
				CountryName = countryName,
				IsBlocked = isBlocked,
				UserAgent = userAgent,
				Timestamp = DateTime.UtcNow
			});
		}

		public PagedResponse<BlockedAttemptLog> GetLogs(int page, int pageSize)
		{
			page = Math.Max(1, page);
			pageSize = Math.Clamp(pageSize, 1, 100);

			var all = _repo.GetAllLogs().ToList();
			var total = all.Count;
			var data = all
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToList();

			return new PagedResponse<BlockedAttemptLog>
			{
				Page = page,
				PageSize = pageSize,
				TotalCount = total,
				Data = data
			};
		}
	}
}
