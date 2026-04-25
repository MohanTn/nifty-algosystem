namespace NiftyOptionsAlgo.Worker.Jobs;

using NiftyOptionsAlgo.Core;
using Quartz;

public class PositionMonitoringJob : IJob
{
    private readonly IPositionMonitor _positionMonitor;
    private readonly ILogger<PositionMonitoringJob> _logger;

    public PositionMonitoringJob(IPositionMonitor positionMonitor, ILogger<PositionMonitoringJob> logger)
    {
        _positionMonitor = positionMonitor;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            _logger.LogDebug("PositionMonitoringJob executing at {time}", DateTime.UtcNow);
            await _positionMonitor.MonitorAllPositionsAsync();
            _logger.LogDebug("Position monitoring completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in PositionMonitoringJob");
        }
    }
}
