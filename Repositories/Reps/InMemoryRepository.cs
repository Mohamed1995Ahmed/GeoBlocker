using Models.Models;
using Repositories.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Reps
{
	public class InMemoryRepository : IInMemoryRepository
	{
		// Thread-safe dictionaries keyed by UPPER-CASE country code
		private readonly ConcurrentDictionary<string, BlockedCountry> _blockedCountries = new();
		private readonly ConcurrentDictionary<string, TemporalBlock> _temporalBlocks = new();

		// Thread-safe log list
		private readonly ConcurrentQueue<BlockedAttemptLog> _logs = new();

		// ── Permanent blocks ──────────────────────────────────────────────────────

		public bool AddBlock(BlockedCountry country)
		{
			var key = country.CountryCode.ToUpperInvariant();
			country.CountryCode = key;
			return _blockedCountries.TryAdd(key, country);   // false = duplicate
		}

		public bool RemoveBlock(string countryCode)
			=> _blockedCountries.TryRemove(countryCode.ToUpperInvariant(), out _);

		public bool IsBlocked(string countryCode)
			=> _blockedCountries.ContainsKey(countryCode.ToUpperInvariant());

		public IEnumerable<BlockedCountry> GetAllBlocked()
			=> _blockedCountries.Values;

		// ── Temporal blocks ───────────────────────────────────────────────────────

		public bool AddTemporalBlock(TemporalBlock block)
		{
			var key = block.CountryCode.ToUpperInvariant();
			block.CountryCode = key;
			return _temporalBlocks.TryAdd(key, block);       // false = duplicate
		}

		public bool HasTemporalBlock(string countryCode)
			=> _temporalBlocks.ContainsKey(countryCode.ToUpperInvariant());

		public bool IsTemporallyBlocked(string countryCode)
		{
			var key = countryCode.ToUpperInvariant();
			if (_temporalBlocks.TryGetValue(key, out var block))
				return block.ExpiresAt > DateTime.UtcNow;
			return false;
		}

		public IEnumerable<TemporalBlock> GetAllTemporalBlocks()
			=> _temporalBlocks.Values;

		/// <summary>Called by the background service every 5 minutes.</summary>
		public int RemoveExpiredTemporalBlocks()
		{
			var expired = _temporalBlocks
				.Where(kv => kv.Value.ExpiresAt <= DateTime.UtcNow)
				.Select(kv => kv.Key)
				.ToList();

			foreach (var key in expired)
				_temporalBlocks.TryRemove(key, out _);

			return expired.Count;
		}

		// ── Logs ──────────────────────────────────────────────────────────────────

		public void AddLog(BlockedAttemptLog log) => _logs.Enqueue(log);

		public IEnumerable<BlockedAttemptLog> GetAllLogs()
			=> _logs.OrderByDescending(l => l.Timestamp);
	}
}
