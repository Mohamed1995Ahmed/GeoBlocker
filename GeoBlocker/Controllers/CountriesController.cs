using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DTO;
using Models.Models;
using Services.Interfaces;

namespace GeoBlocker.Controllers
{

	[ApiController]
	[Route("api/countries")]
	[Produces("application/json")]
	public class CountriesController : ControllerBase
	{
		private readonly ICountryService _countryService;
		private readonly IGeoLocationService _geoService;
		private readonly ILogger<CountriesController> _logger;

		public CountriesController(
			ICountryService countryService,
			IGeoLocationService geoService,
			ILogger<CountriesController> logger)
		{
			_countryService = countryService;
			_geoService = geoService;
			_logger = logger;
		}

		// ── 1. Add a blocked country ───────────────────────────────────────────────

		/// <summary>Permanently block a country by its ISO code.</summary>
		[HttpPost("block")]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		public async Task<IActionResult> AddBlock([FromBody] BlockCountryRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.CountryCode))
				return BadRequest(new { message = "CountryCode is required." });

			// Resolve country name via GeoLocation (lookup a dummy IP is not ideal;
			// we use a known IP for the country or just store the code and name from a lookup).
			// For simplicity we accept an optional countryName; if not provided we try to fetch it.
			var countryName = await ResolveCountryNameAsync(request.CountryCode);

			var (success, message) = _countryService.AddBlock(request.CountryCode, countryName);

			if (!success)
			{
				// Determine whether it's a duplicate (409) or invalid code (400)
				if (message.Contains("already blocked"))
					return Conflict(new { message });
				return BadRequest(new { message });
			}

			return StatusCode(StatusCodes.Status201Created, new { message });
		}

		// ── 2. Delete a blocked country ───────────────────────────────────────────

		/// <summary>Remove a country from the permanent block list.</summary>
		[HttpDelete("block/{countryCode}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public IActionResult RemoveBlock(string countryCode)
		{
			var (success, message) = _countryService.RemoveBlock(countryCode);

			if (!success)
				return NotFound(new { message });

			return Ok(new { message });
		}

		// ── 3. Get all blocked countries (paginated + search) ─────────────────────

		/// <summary>
		/// Returns all permanently blocked countries.
		/// Supports pagination (page, pageSize) and search/filter by code or name.
		/// </summary>
		[HttpGet("blocked")]
		[ProducesResponseType(typeof(PagedResponse<BlockedCountry>), StatusCodes.Status200OK)]
		public IActionResult GetAllBlocked(
			[FromQuery] int page = 1,
			[FromQuery] int pageSize = 10,
			[FromQuery] string? search = null)
		{
			var result = _countryService.GetAllBlocked(page, pageSize, search);
			return Ok(result);
		}

		// ── 7. Temporarily block a country ────────────────────────────────────────

		/// <summary>Block a country for a specific duration (1–1440 minutes).</summary>
		[HttpPost("temporal-block")]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		public async Task<IActionResult> AddTemporalBlock([FromBody] TemporalBlockRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.CountryCode))
				return BadRequest(new { message = "CountryCode is required." });

			var countryName = await ResolveCountryNameAsync(request.CountryCode);

			var (success, message, statusCode) = _countryService.AddTemporalBlock(
				request.CountryCode, countryName, request.DurationMinutes);

			return StatusCode(statusCode, new { message });
		}

		// ── Helper ─────────────────────────────────────────────────────────────────

		/// <summary>
		/// Best-effort resolution of a country name from the country code.
		/// Uses a static fallback dictionary; the GeoLocation API is not called here
		/// because we don't have an IP to look up — only a country code.
		/// </summary>
		private Task<string> ResolveCountryNameAsync(string code)
		{
			// Static map for the most common codes
			var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
				["EG"] = "Egypt",
				["US"] = "United States",
				["GB"] = "United Kingdom",
				["DE"] = "Germany",
				["FR"] = "France",
				["CN"] = "China",
				["RU"] = "Russia",
				["IN"] = "India",
				["BR"] = "Brazil",
				["JP"] = "Japan",
				["KR"] = "South Korea",
				["AU"] = "Australia",
				["CA"] = "Canada",
				["SA"] = "Saudi Arabia",
				["AE"] = "United Arab Emirates",
				["TR"] = "Turkey",
				["IR"] = "Iran",
				["IQ"] = "Iraq",
				["IL"] = "Israel",
				["PK"] = "Pakistan",
				["NG"] = "Nigeria",
				["ZA"] = "South Africa",
				["MX"] = "Mexico",
				["AR"] = "Argentina",
				["IT"] = "Italy",
				["ES"] = "Spain",
				["PL"] = "Poland",
				["NL"] = "Netherlands",
				["SE"] = "Sweden",
				["NO"] = "Norway",
			};

			var name = map.TryGetValue(code.ToUpperInvariant(), out var n) ? n : code.ToUpperInvariant();
			return Task.FromResult(name);
		}
	}
}