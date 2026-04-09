using Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
	public interface IInMemoryRepository
	{
		// Permanent blocks
		bool AddBlock(BlockedCountry country);
		bool RemoveBlock(string countryCode);
		bool IsBlocked(string countryCode);
		IEnumerable<BlockedCountry> GetAllBlocked();

		// Temporal blocks
		bool AddTemporalBlock(TemporalBlock block);
		bool HasTemporalBlock(string countryCode);
		bool IsTemporallyBlocked(string countryCode);
		IEnumerable<TemporalBlock> GetAllTemporalBlocks();
		int RemoveExpiredTemporalBlocks();

		// Logs
		void AddLog(BlockedAttemptLog log);
		IEnumerable<BlockedAttemptLog> GetAllLogs();
	}
}
