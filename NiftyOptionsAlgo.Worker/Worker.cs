namespace NiftyOptionsAlgo.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Quartz scheduler worker started at: {time}", DateTimeOffset.Now);
        await Task.CompletedTask;
    }
}
