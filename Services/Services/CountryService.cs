using Microsoft.Extensions.Logging;
using Models.DTO;
using Models.Models;
using Services.Interfaces;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Services
{
	public class CountryService : ICountryService
	{
		private readonly IInMemoryRepository _repo;
		private readonly ILogger<CountryService> _logger;

		// Simple set of valid ISO 3166-1 alpha-2 codes for validation
		private static readonly HashSet<string> ValidCountryCodes = new(StringComparer.OrdinalIgnoreCase)
	{
		"AF","AX","AL","DZ","AS","AD","AO","AI","AQ","AG","AR","AM","AW","AU","AT",
		"AZ","BS","BH","BD","BB","BY","BE","BZ","BJ","BM","BT","BO","BQ","BA","BW",
		"BV","BR","IO","BN","BG","BF","BI","CV","KH","CM","CA","KY","CF","TD","CL",
		"CN","CX","CC","CO","KM","CG","CD","CK","CR","CI","HR","CU","CW","CY","CZ",
		"DK","DJ","DM","DO","EC","EG","SV","GQ","ER","EE","SZ","ET","FK","FO","FJ",
		"FI","FR","GF","PF","TF","GA","GM","GE","DE","GH","GI","GR","GL","GD","GP",
		"GU","GT","GG","GN","GW","GY","HT","HM","VA","HN","HK","HU","IS","IN","ID",
		"IR","IQ","IE","IM","IL","IT","JM","JP","JE","JO","KZ","KE","KI","KP","KR",
		"KW","KG","LA","LV","LB","LS","LR","LY","LI","LT","LU","MO","MG","MW","MY",
		"MV","ML","MT","MH","MQ","MR","MU","YT","MX","FM","MD","MC","MN","ME","MS",
		"MA","MZ","MM","NA","NR","NP","NL","NC","NZ","NI","NE","NG","NU","NF","MK",
		"MP","NO","OM","PK","PW","PS","PA","PG","PY","PE","PH","PN","PL","PT","PR",
		"QA","RE","RO","RU","RW","BL","SH","KN","LC","MF","PM","VC","WS","SM","ST",
		"SA","SN","RS","SC","SL","SG","SX","SK","SI","SB","SO","ZA","GS","SS","ES",
		"LK","SD","SR","SJ","SE","CH","SY","TW","TJ","TZ","TH","TL","TG","TK","TO",
		"TT","TN","TR","TM","TC","TV","UG","UA","AE","GB","US","UM","UY","UZ","VU",
		"VE","VN","VG","VI","WF","EH","YE","ZM","ZW"
	};

		public CountryService(IInMemoryRepository repo, ILogger<CountryService> logger)
		{
			_repo = repo;
			_logger = logger;
		}

		// ── Permanent blocks ──────────────────────────────────────────────────────

		public (bool Success, string Message) AddBlock(string countryCode, string countryName)
		{
			var code = countryCode.ToUpperInvariant();

			if (!ValidCountryCodes.Contains(code))
				return (false, $"Invalid country code '{code}'.");

			var country = new BlockedCountry
			{
				CountryCode = code,
				CountryName = countryName,
				BlockedAt = DateTime.UtcNow
			};

			var added = _repo.AddBlock(country);
			if (!added)
				return (false, $"Country '{code}' is already blocked.");

			_logger.LogInformation("Country '{Code}' added to blocked list.", code);
			return (true, $"Country '{code}' has been blocked successfully.");
		}

		public (bool Success, string Message) RemoveBlock(string countryCode)
		{
			var code = countryCode.ToUpperInvariant();
			var removed = _repo.RemoveBlock(code);

			if (!removed)
				return (false, $"Country '{code}' was not found in the blocked list.");

			_logger.LogInformation("Country '{Code}' removed from blocked list.", code);
			return (true, $"Country '{code}' has been unblocked.");
		}

		public PagedResponse<BlockedCountry> GetAllBlocked(int page, int pageSize, string? search)
		{
			page = Math.Max(1, page);
			pageSize = Math.Clamp(pageSize, 1, 100);

			var query = _repo.GetAllBlocked().AsQueryable();

			if (!string.IsNullOrWhiteSpace(search))
			{
				var s = search.Trim().ToUpperInvariant();
				query = query.Where(c =>
					c.CountryCode.Contains(s, StringComparison.OrdinalIgnoreCase) ||
					c.CountryName.Contains(s, StringComparison.OrdinalIgnoreCase));
			}

			var total = query.Count();
			var data = query
				.OrderBy(c => c.CountryCode)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToList();

			return new PagedResponse<BlockedCountry>
			{
				Page = page,
				PageSize = pageSize,
				TotalCount = total,
				Data = data
			};
		}

		// ── Temporal blocks ───────────────────────────────────────────────────────

		public (bool Success, string Message, int StatusCode) AddTemporalBlock(
			string countryCode, string countryName, int durationMinutes)
		{
			var code = countryCode.ToUpperInvariant();

			if (!ValidCountryCodes.Contains(code))
				return (false, $"Invalid country code '{code}'.", 400);

			if (durationMinutes < 1 || durationMinutes > 1440)
				return (false, "durationMinutes must be between 1 and 1440.", 400);

			if (_repo.HasTemporalBlock(code))
				return (false, $"Country '{code}' is already temporarily blocked.", 409);

			var block = new TemporalBlock
			{
				CountryCode = code,
				CountryName = countryName,
				BlockedAt = DateTime.UtcNow,
				ExpiresAt = DateTime.UtcNow.AddMinutes(durationMinutes),
				DurationMinutes = durationMinutes
			};

			_repo.AddTemporalBlock(block);
			_logger.LogInformation("Country '{Code}' temporarily blocked for {Min} minutes.", code, durationMinutes);
			return (true, $"Country '{code}' has been temporarily blocked for {durationMinutes} minutes.", 201);
		}

		public IEnumerable<TemporalBlock> GetAllTemporalBlocks()
			=> _repo.GetAllTemporalBlocks();

		// ── Combined check ────────────────────────────────────────────────────────

		public bool IsBlockedByAny(string countryCode)
			=> _repo.IsBlocked(countryCode) || _repo.IsTemporallyBlocked(countryCode);
	}
}
