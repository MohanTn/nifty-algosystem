namespace NiftyOptionsAlgo.Worker.Jobs;

using NiftyOptionsAlgo.Core;
using Quartz;

public class HealthCheckJob : IJob
{
    private readonly IKiteService _kiteService;
    private readonly ILogger<HealthCheckJob> _logger;

    public HealthCheckJob(IKiteService kiteService, ILogger<HealthCheckJob> logger)
    {
        _kiteService = kiteService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            _logger.LogDebug("HealthCheckJob executing at {time}", DateTime.UtcNow);

            // Check API connectivity and session validity
            var isSessionValid = await _kiteService.IsSessionValidAsync();

            if (isSessionValid)
            {
                _logger.LogDebug("Health check passed: API session valid");
            }
            else
            {
                _logger.LogError("Health check failed: API session invalid");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in HealthCheckJob");
        }
    }
}
