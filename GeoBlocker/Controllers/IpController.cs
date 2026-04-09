using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Services.Interfaces;

namespace GeoBlocker.Controllers { 

[ApiController]
[Route("api/ip")]
[Produces("application/json")]
public class IpController : ControllerBase
{
	private readonly IGeoLocationService _geoService;
	private readonly ICountryService _countryService;
	private readonly ILogService _logService;
	private readonly ILogger<IpController> _logger;

	public IpController(
		IGeoLocationService geoService,
		ICountryService countryService,
		ILogService logService,
		ILogger<IpController> logger)
	{
		_geoService = geoService;
		_countryService = countryService;
		_logService = logService;
		_logger = logger;
	}

	// ── 4. Find my country via IP lookup ──────────────────────────────────────

	/// <summary>
	/// Returns geolocation details for an IP address.
	/// If ipAddress is omitted, the caller's IP is used automatically.
	/// </summary>
	[HttpGet("lookup")]
	[ProducesResponseType(typeof(IpLookupResult), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status502BadGateway)]
	public async Task<IActionResult> Lookup([FromQuery] string? ipAddress = null)
	{
		// If no IP supplied, use the caller's IP
		var targetIp = string.IsNullOrWhiteSpace(ipAddress)
			? GetCallerIp()
			: ipAddress.Trim();

		if (!_geoService.IsValidIpFormat(targetIp))
			return BadRequest(new { message = $"'{targetIp}' is not a valid IP address format." });

		var result = await _geoService.LookupAsync(targetIp);

		if (result is null)
			return StatusCode(StatusCodes.Status502BadGateway,
				new { message = "Failed to retrieve geolocation data. Please try again later." });

		return Ok(result);
	}

	// ── 5. Check if caller's IP is blocked ────────────────────────────────────

	/// <summary>
	/// Checks whether the caller's country is in the blocked list.
	/// The attempt is always logged.
	/// </summary>
	[HttpGet("check-block")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status502BadGateway)]
	public async Task<IActionResult> CheckBlock()
	{
		var callerIp = GetCallerIp();
		var userAgent = Request.Headers.UserAgent.ToString();

		_logger.LogInformation("check-block called from IP: {Ip}", callerIp);
			//if (!_geoService.IsValidIpFormat(callerIp) || IsPrivateIp(callerIp))
			//	return BadRequest(new { message = "Cannot determine geolocation for a private or loopback IP." });


			// Step 1 – Geo-lookup the caller's IP
			var geoResult = await _geoService.LookupAsync(callerIp);

		if (geoResult is null)
			return StatusCode(StatusCodes.Status502BadGateway,
				new { message = "Unable to determine caller's country. Please try again." });

		// Step 2 – Check blocked list (permanent + temporal)
		var isBlocked = _countryService.IsBlockedByAny(geoResult.CountryCode);

		// Step 3 – Log the attempt
		_logService.LogAttempt(callerIp, geoResult.CountryCode, geoResult.CountryName, isBlocked, userAgent);

		if (isBlocked)
		{
			return StatusCode(StatusCodes.Status403Forbidden, new
			{
				message = "Access denied. Your country is blocked.",
				ip = callerIp,
				countryCode = geoResult.CountryCode,
				countryName = geoResult.CountryName
			});
		}

		return Ok(new
		{
			message = "Access allowed.",
			ip = callerIp,
			countryCode = geoResult.CountryCode,
			countryName = geoResult.CountryName
		});
	}

		// ── Helper ─────────────────────────────────────────────────────────────────

		/// <summary>
		/// Extracts the real caller IP, respecting X-Forwarded-For from reverse proxies.
		/// </summary>
		//private string GetCallerIp()
		//{
		//	// Check X-Forwarded-For header first (set by reverse proxies / load balancers)
		//	var forwarded = Request.Headers["X-Forwarded-For"].FirstOrDefault();
		//	if (!string.IsNullOrWhiteSpace(forwarded))
		//	{
		//		// May contain a comma-separated list; take the first (original client)
		//		var first = forwarded.Split(',').FirstOrDefault()?.Trim();
		//		if (!string.IsNullOrWhiteSpace(first) && _geoService.IsValidIpFormat(first))
		//			return first;
		//	}

		//	// Fall back to the direct connection IP
		//	return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
		//}
		//private static bool IsPrivateIp(string ip)
		//{
		//	if (ip == "::1" || ip == "127.0.0.1") return true;

		//	if (System.Net.IPAddress.TryParse(ip, out var addr))
		//	{
		//		var bytes = addr.GetAddressBytes();
		//		return bytes[0] == 10 ||
		//			   (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
		//			   (bytes[0] == 192 && bytes[1] == 168);
		//	}
		//	return false;
		//}
		private string GetCallerIp()
		{
			var forwarded = Request.Headers["X-Forwarded-For"].FirstOrDefault();
			if (!string.IsNullOrWhiteSpace(forwarded))
			{
				var first = forwarded.Split(',').FirstOrDefault()?.Trim();
				if (!string.IsNullOrWhiteSpace(first) && _geoService.IsValidIpFormat(first))
					return first;
			}

			var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

			// If loopback, substitute a real public IP for dev/testing
			if (ip == "::1" || ip == "127.0.0.1")
				ip = "8.8.8.8"; // or any known public IP for testing

			return ip;
		}
	}
}
