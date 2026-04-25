namespace NiftyOptionsAlgo.Backtester;

using Microsoft.Extensions.Logging;

public class MonteCarloSimulator
{
    private readonly ILogger<MonteCarloSimulator> _logger;
    private readonly Random _random = new();

    public MonteCarloSimulator(ILogger<MonteCarloSimulator> logger)
    {
        _logger = logger;
    }

    public List<BacktestResult> SimulateMonteCarlo(
        List<TradeExecution> historicalTrades,
        BacktestConfiguration config,
        decimal dailyVolatility)
    {
        _logger.LogInformation("Running Monte Carlo simulation with {simulations} iterations",
            config.NumberOfSimulations);

        var results = new List<BacktestResult>();

        for (int sim = 0; sim < config.NumberOfSimulations; sim++)
        {
            var result = new BacktestResult
            {
                RunDate = DateTime.UtcNow,
                InitialCapital = config.InitialCapital,
                FinalCapital = config.InitialCapital
            };

            var capital = config.InitialCapital;
            var randomizedTrades = RandomizeTradeSequence(historicalTrades);

            foreach (var trade in randomizedTrades)
            {
                var scaledPnL = GenerateScaledPnL(trade.PnL, dailyVolatility);
                capital += scaledPnL;

                result.Trades.Add(new TradeExecution
                {
                    ExecutionDate = trade.ExecutionDate,
                    Symbol = trade.Symbol,
                    EntryPrice = trade.EntryPrice,
                    ExitPrice = trade.ExitPrice,
                    ExitDate = trade.ExitDate,
                    Quantity = trade.Quantity,
                    PnL = scaledPnL,
                    PnLPercent = scaledPnL / (trade.EntryPrice * trade.Quantity)
                });
            }

            result.FinalCapital = capital;
            result.TotalReturn = capital - config.InitialCapital;
            result.TotalReturnPercent = result.TotalReturn / config.InitialCapital;
            result.TotalTrades = result.Trades.Count;
            result.WinningTrades = result.Trades.Count(t => t.PnL > 0);
            result.LosingTrades = result.Trades.Count(t => t.PnL <= 0);

            if (result.TotalTrades > 0)
            {
                result.WinRate = (decimal)result.WinningTrades / result.TotalTrades;
            }

            results.Add(result);
        }

        _logger.LogInformation("Monte Carlo simulation complete: {simulations} iterations",
            config.NumberOfSimulations);

        return results;
    }

    public MonteCarloStatistics AnalyzeMonteCarlo(List<BacktestResult> results)
    {
        var finalCapitals = results.Select(r => r.FinalCapital).OrderBy(x => x).ToList();
        var returns = results.Select(r => r.TotalReturnPercent).ToList();

        var stats = new MonteCarloStatistics
        {
            SimulationCount = results.Count,
            MeanCapital = finalCapitals.Average(),
            MedianCapital = finalCapitals[finalCapitals.Count / 2],
            MinCapital = finalCapitals.First(),
            MaxCapital = finalCapitals.Last(),
            StdDevCapital = CalculateStandardDeviation(finalCapitals),
            ValueAtRisk = finalCapitals[(int)(finalCapitals.Count * 0.05)],
            ConditionalValueAtRisk = finalCapitals.Take((int)(finalCapitals.Count * 0.05)).Average(),
            WinRate = (decimal)results.Count(r => r.FinalCapital > r.InitialCapital) / results.Count,
            AverageReturn = returns.Average(),
            StdDevReturn = CalculateStandardDeviation(returns),
            MinReturn = returns.Min(),
            MaxReturn = returns.Max()
        };

        return stats;
    }

    private List<TradeExecution> RandomizeTradeSequence(List<TradeExecution> trades)
    {
        var shuffled = new List<TradeExecution>(trades);
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int randomIndex = _random.Next(i + 1);
            (shuffled[i], shuffled[randomIndex]) = (shuffled[randomIndex], shuffled[i]);
        }
        return shuffled;
    }

    private decimal GenerateScaledPnL(decimal basePnL, decimal volatility)
    {
        // Box-Muller transform for normal distribution
        double u1 = _random.NextDouble();
        double u2 = _random.NextDouble();
        double z = Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);

        return basePnL * (1 + (decimal)z * volatility);
    }

    private decimal CalculateStandardDeviation(List<decimal> values)
    {
        if (values.Count < 2) return 0;

        var mean = values.Average();
        var variance = values.Sum(v => (v - mean) * (v - mean)) / (values.Count - 1);
        return (decimal)Math.Sqrt((double)variance);
    }
}

public class MonteCarloStatistics
{
    public int SimulationCount { get; set; }
    public decimal MeanCapital { get; set; }
    public decimal MedianCapital { get; set; }
    public decimal MinCapital { get; set; }
    public decimal MaxCapital { get; set; }
    public decimal StdDevCapital { get; set; }
    public decimal ValueAtRisk { get; set; }
    public decimal ConditionalValueAtRisk { get; set; }
    public decimal WinRate { get; set; }
    public decimal AverageReturn { get; set; }
    public decimal StdDevReturn { get; set; }
    public decimal MinReturn { get; set; }
    public decimal MaxReturn { get; set; }
}
