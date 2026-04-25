namespace NiftyOptionsAlgo.Worker.Jobs;

using NiftyOptionsAlgo.Core;
using Quartz;

public class RiskAssessmentJob : IJob
{
    private readonly IRiskManager _riskManager;
    private readonly ILogger<RiskAssessmentJob> _logger;

    public RiskAssessmentJob(IRiskManager riskManager, ILogger<RiskAssessmentJob> logger)
    {
        _riskManager = riskManager;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            _logger.LogDebug("RiskAssessmentJob executing at {time}", DateTime.UtcNow);
            var assessment = await _riskManager.AssessPortfolioRiskAsync();

            if (assessment.IsWithinLimits)
            {
                _logger.LogInformation("Portfolio risk within limits. Drawdown: {drawdown:P2}",
                    assessment.CurrentDrawdown);
            }
            else
            {
                _logger.LogWarning("Portfolio risk exceeds limits! Drawdown: {drawdown:P2}",
                    assessment.CurrentDrawdown);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RiskAssessmentJob");
        }
    }
}
