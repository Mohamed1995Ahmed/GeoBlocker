using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Models.DTO;
using Models.Models;
using Newtonsoft.Json;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Services
{
	public class GeoLocationService : IGeoLocationService
	{
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _config;
		private readonly ILogger<GeoLocationService> _logger;

		public GeoLocationService(
			IHttpClientFactory httpClientFactory,
			IConfiguration config,
			ILogger<GeoLocationService> logger)
		{
			_httpClient = httpClientFactory.CreateClient("GeoApi");
			_config = config;
			_logger = logger;
		}

		/// <summary>Calls ipapi.co/{ip}/json and maps to our model.</summary>
		public async Task<IpLookupResult?> LookupAsync(string ipAddress)
		{
			try
			{
				var baseUrl = _config["GeoLocation:IpApiBaseUrl"]?.TrimEnd('/')
							  ?? "https://api.ipgeolocation.io";
				var apiKey = _config["GeoLocation:ApiKey"];

				var url = $"{baseUrl}/ipgeo?apiKey={apiKey}&ip={ipAddress}";

				_logger.LogInformation("Calling GeoLocation API for IP: {Ip}", ipAddress);

				var response = await _httpClient.GetAsync(url);
				response.EnsureSuccessStatusCode();

				var json = await response.Content.ReadAsStringAsync();
				var raw = JsonConvert.DeserializeObject<IpapiResponse>(json);

				if (raw is null)
				{
					_logger.LogWarning("GeoLocation API returned null for {Ip}", ipAddress);
					return null;
				}

				return new IpLookupResult
				{
					IpAddress = raw.ip ?? ipAddress,
					CountryCode = raw.country_code2 ?? string.Empty,
					CountryName = raw.country_name ?? string.Empty,
					City = raw.city ?? string.Empty,
					Region = raw.state_prov ?? string.Empty,
					Isp = raw.isp ?? string.Empty,
					Timezone = raw.time_zone?.name ?? string.Empty
				};
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, "HTTP error calling GeoLocation API for {Ip}", ipAddress);
				return null;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unexpected error in GeoLocationService for {Ip}", ipAddress);
				return null;
			}
		}

		/// <summary>Validates IPv4 and IPv6 formats.</summary>
		public bool IsValidIpFormat(string ipAddress)
		{
			if (string.IsNullOrWhiteSpace(ipAddress)) return false;
			return System.Net.IPAddress.TryParse(ipAddress, out _);
		}
	}

}
