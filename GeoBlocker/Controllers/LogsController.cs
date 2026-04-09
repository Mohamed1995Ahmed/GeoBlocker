using Microsoft.AspNetCore.Mvc;
using Models.DTO;
using Models.Models;
using Services.Interfaces;

namespace GeoBlocker.Controllers
{

	[ApiController]
	[Route("api/logs")]
	[Produces("application/json")]
	public class LogsController : ControllerBase
	{
		private readonly ILogService _logService;

		public LogsController(ILogService logService) => _logService = logService;

		// ── 6. Get blocked attempt logs ───────────────────────────────────────────

		/// <summary>
		/// Returns a paginated list of all IP check-block attempts (blocked or allowed).
		/// Results are ordered by most recent first.
		/// </summary>
		[HttpGet("blocked-attempts")]
		[ProducesResponseType(typeof(PagedResponse<BlockedAttemptLog>), StatusCodes.Status200OK)]
		public IActionResult GetBlockedAttempts(
			[FromQuery] int page = 1,
			[FromQuery] int pageSize = 10)
		{
			var result = _logService.GetLogs(page, pageSize);
			return Ok(result);
		}
	}
}
