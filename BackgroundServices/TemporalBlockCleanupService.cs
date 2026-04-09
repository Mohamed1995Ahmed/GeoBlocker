using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Repositories.Interfaces;

namespace BackgroundServices
{
	public class TemporalBlockCleanupService : BackgroundService
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly ILogger<TemporalBlockCleanupService> _logger;
		private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

		public TemporalBlockCleanupService(
			IServiceProvider serviceProvider,
			ILogger<TemporalBlockCleanupService> logger)
		{
			_serviceProvider = serviceProvider;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("TemporalBlockCleanupService started. Interval: {Interval}", Interval);

			while (!stoppingToken.IsCancellationRequested)
			{
				await Task.Delay(Interval, stoppingToken);

				try
				{
					// IInMemoryRepository is registered as Singleton, so we can resolve it directly.
					using var scope = _serviceProvider.CreateScope();
					var repo = scope.ServiceProvider.GetRequiredService<IInMemoryRepository>();

					var removed = repo.RemoveExpiredTemporalBlocks();
					if (removed > 0)
						_logger.LogInformation("Cleanup: removed {Count} expired temporal block(s).", removed);
					else
						_logger.LogDebug("Cleanup: no expired temporal blocks found.");
				}
				catch (OperationCanceledException)
				{
					// Normal shutdown
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error during temporal block cleanup.");
				}
			}

			_logger.LogInformation("TemporalBlockCleanupService stopping.");
		}
	}
}
