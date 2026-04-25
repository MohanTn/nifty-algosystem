namespace NiftyOptionsAlgo.Worker.Jobs;

using NiftyOptionsAlgo.Core;
using NiftyOptionsAlgo.Engine;
using Quartz;

public class EntryEvaluationJob : IJob
{
    private readonly IStrategyEngine _strategyEngine;
    private readonly ILogger<EntryEvaluationJob> _logger;

    public EntryEvaluationJob(IStrategyEngine strategyEngine, ILogger<EntryEvaluationJob> logger)
    {
        _strategyEngine = strategyEngine;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            _logger.LogInformation("EntryEvaluationJob executing at {time}", DateTime.UtcNow);
            var result = await _strategyEngine.EvaluateEntryAsync();

            if (result.ShouldEnter)
            {
                _logger.LogInformation("Entry conditions met. Strategy: {strategy}, Lots: {lots}",
                    result.RecommendedStrategy, result.RecommendedLots);
            }
            else
            {
                _logger.LogDebug("Entry conditions not met. Failed checks: {checks}",
                    string.Join(", ", result.FailedChecks));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in EntryEvaluationJob");
            throw;
        }
    }
}
