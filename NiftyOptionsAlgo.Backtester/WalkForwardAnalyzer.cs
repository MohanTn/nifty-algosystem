namespace NiftyOptionsAlgo.Backtester;

using Microsoft.Extensions.Logging;

public class WalkForwardAnalyzer
{
    private readonly ILogger<WalkForwardAnalyzer> _logger;

    public WalkForwardAnalyzer(ILogger<WalkForwardAnalyzer> logger)
    {
        _logger = logger;
    }

    public WalkForwardResult AnalyzeWalkForward(
        HistoricalData historicalData,
        BacktestConfiguration config,
        Func<HistoricalData, BacktestConfiguration, Task<BacktestResult>> optimizeAndTest)
    {
        _logger.LogInformation("Starting walk-forward analysis with {days} day optimization and {days} day test periods",
            config.WalkForwardPeriodDays, config.TestPeriodDays);

        var result = new WalkForwardResult
        {
            AnalysisDate = DateTime.UtcNow,
            TotalTrades = 0,
            InSampleTrades = 0,
            OutOfSampleTrades = 0
        };

        var startDate = config.StartDate;
        var endDate = config.EndDate;
        var optimizationWindowSize = config.WalkForwardPeriodDays;
        var testWindowSize = config.TestPeriodDays;

        while (startDate.AddDays(optimizationWindowSize + testWindowSize) <= endDate)
        {
            var optimizationEnd = startDate.AddDays(optimizationWindowSize);
            var testEnd = optimizationEnd.AddDays(testWindowSize);

            // Optimize on in-sample period
            var inSampleData = new HistoricalData
            {
                Symbol = historicalData.Symbol,
                StartDate = startDate,
                EndDate = optimizationEnd,
                Bars = historicalData.Bars
                    .Where(b => b.Timestamp >= startDate && b.Timestamp <= optimizationEnd)
                    .ToList()
            };

            var inSampleConfig = new BacktestConfiguration
            {
                StartDate = startDate,
                EndDate = optimizationEnd,
                InitialCapital = config.InitialCapital,
                IncludeTransactionCosts = config.IncludeTransactionCosts,
                TransactionCostPercent = config.TransactionCostPercent
            };

            var inSampleResult = optimizeAndTest(inSampleData, inSampleConfig).Result;

            // Test on out-of-sample period
            var outOfSampleData = new HistoricalData
            {
                Symbol = historicalData.Symbol,
                StartDate = optimizationEnd,
                EndDate = testEnd,
                Bars = historicalData.Bars
                    .Where(b => b.Timestamp > optimizationEnd && b.Timestamp <= testEnd)
                    .ToList()
            };

            var outOfSampleConfig = new BacktestConfiguration
            {
                StartDate = optimizationEnd,
                EndDate = testEnd,
                InitialCapital = config.InitialCapital,
                IncludeTransactionCosts = config.IncludeTransactionCosts,
                TransactionCostPercent = config.TransactionCostPercent
            };

            var outOfSampleResult = optimizeAndTest(outOfSampleData, outOfSampleConfig).Result;

            result.Windows.Add(new WalkForwardWindow
            {
                OptimizationStart = startDate,
                OptimizationEnd = optimizationEnd,
                TestStart = optimizationEnd,
                TestEnd = testEnd,
                InSampleResult = inSampleResult,
                OutOfSampleResult = outOfSampleResult,
                DegradationRatio = outOfSampleResult.TotalReturnPercent > 0
                    ? inSampleResult.TotalReturnPercent / outOfSampleResult.TotalReturnPercent
                    : 0
            });

            result.InSampleTrades += inSampleResult.TotalTrades;
            result.OutOfSampleTrades += outOfSampleResult.TotalTrades;
            result.TotalTrades += inSampleResult.TotalTrades + outOfSampleResult.TotalTrades;

            startDate = testEnd;
        }

        // Calculate aggregate metrics
        if (result.Windows.Count > 0)
        {
            var avgDegradation = result.Windows.Average(w => w.DegradationRatio);
            result.AvgDegradationRatio = avgDegradation;

            var inSampleReturn = result.Windows.Average(w => w.InSampleResult.TotalReturnPercent);
            var outOfSampleReturn = result.Windows.Average(w => w.OutOfSampleResult.TotalReturnPercent);
            result.InSampleReturn = inSampleReturn;
            result.OutOfSampleReturn = outOfSampleReturn;

            result.IsRobust = outOfSampleReturn > 0 && avgDegradation < 2.0m;
        }

        _logger.LogInformation("Walk-forward analysis complete: {windows} windows, robust={robust}",
            result.Windows.Count, result.IsRobust);

        return result;
    }
}

public class WalkForwardResult
{
    public DateTime AnalysisDate { get; set; }
    public List<WalkForwardWindow> Windows { get; set; } = new();
    public int TotalTrades { get; set; }
    public int InSampleTrades { get; set; }
    public int OutOfSampleTrades { get; set; }
    public decimal InSampleReturn { get; set; }
    public decimal OutOfSampleReturn { get; set; }
    public decimal AvgDegradationRatio { get; set; }
    public bool IsRobust { get; set; }
}

public class WalkForwardWindow
{
    public DateTime OptimizationStart { get; set; }
    public DateTime OptimizationEnd { get; set; }
    public DateTime TestStart { get; set; }
    public DateTime TestEnd { get; set; }
    public BacktestResult InSampleResult { get; set; } = new();
    public BacktestResult OutOfSampleResult { get; set; } = new();
    public decimal DegradationRatio { get; set; }
}
