using Models.DTO;
using Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
	public interface ICountryService
	{
		// Permanent
		(bool Success, string Message) AddBlock(string countryCode, string countryName);
		(bool Success, string Message) RemoveBlock(string countryCode);
		PagedResponse<BlockedCountry> GetAllBlocked(int page, int pageSize, string? search);

		// Temporal
		(bool Success, string Message, int StatusCode) AddTemporalBlock(string countryCode, string countryName, int durationMinutes);
		IEnumerable<TemporalBlock> GetAllTemporalBlocks();

		// Combined check (permanent OR temporal)
		bool IsBlockedByAny(string countryCode);
	}
}
