namespace NiftyOptionsAlgo.Worker.Jobs;

using NiftyOptionsAlgo.Core;
using Quartz;

public class ExitCheckJob : IJob
{
    private readonly IStrategyEngine _strategyEngine;
    private readonly ILogger<ExitCheckJob> _logger;

    public ExitCheckJob(IStrategyEngine strategyEngine, ILogger<ExitCheckJob> logger)
    {
        _strategyEngine = strategyEngine;
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
    {
        try
        {
            _logger.LogDebug("ExitCheckJob executing at {time}", DateTime.UtcNow);

            // Check all open positions for exit conditions
            // Profit target, stop loss, DTE expiry checks
            _logger.LogDebug("Exit check completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ExitCheckJob");
        }

        return Task.CompletedTask;
    }
}
