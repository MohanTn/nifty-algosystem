namespace NiftyOptionsAlgo.Worker.Jobs;

using NiftyOptionsAlgo.Core;
using Quartz;

public class RebalanceJob : IJob
{
    private readonly IRiskManager _riskManager;
    private readonly ILogger<RebalanceJob> _logger;

    public RebalanceJob(IRiskManager riskManager, ILogger<RebalanceJob> logger)
    {
        _riskManager = riskManager;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            _logger.LogInformation("RebalanceJob executing at {time}", DateTime.UtcNow);

            // Weekly portfolio rebalancing logic
            // Reallocate capital across strategy buckets based on performance
            var assessment = await _riskManager.AssessPortfolioRiskAsync();

            _logger.LogInformation("Portfolio rebalance check completed. Drawdown: {drawdown:P2}",
                assessment.CurrentDrawdown);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RebalanceJob");
        }
    }
}
