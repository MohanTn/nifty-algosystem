namespace NiftyOptionsAlgo.Worker.Jobs;

using NiftyOptionsAlgo.Core;
using Quartz;

public class VixUpdateJob : IJob
{
    private readonly IKiteService _kiteService;
    private readonly ILogger<VixUpdateJob> _logger;

    public VixUpdateJob(IKiteService kiteService, ILogger<VixUpdateJob> logger)
    {
        _kiteService = kiteService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            _logger.LogDebug("VixUpdateJob executing at {time}", DateTime.UtcNow);
            var vixQuote = await _kiteService.GetQuoteAsync("NIFTYIT");

            _logger.LogInformation("VIX Update: {price}", vixQuote.LastPrice);

            // In production, this would update a VIX tracking service
            // and trigger regime changes if thresholds are crossed
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in VixUpdateJob");
        }
    }
}
