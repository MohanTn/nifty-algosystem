namespace NiftyOptionsAlgo.Backtester;

using Microsoft.Extensions.Logging;

public interface IBacktestEngine
{
    Task<BacktestResult> RunHistoricalBacktestAsync(
        HistoricalData historicalData,
        BacktestConfiguration config,
        Func<HistoricalBar, decimal, Task<(int quantity, decimal entryPrice, bool shouldEnter)>> entryLogic,
        Func<HistoricalBar, decimal, Task<(decimal exitPrice, bool shouldExit)>> exitLogic);

    Task<List<BacktestResult>> RunMonteCarloAsync(
        List<TradeExecution> trades,
        BacktestConfiguration config,
        decimal dailyVolatility);

    Task<WalkForwardResult> RunWalkForwardAsync(
        HistoricalData historicalData,
        BacktestConfiguration config,
        Func<HistoricalData, BacktestConfiguration, Task<BacktestResult>> optimizeAndTest);
}

public class BacktestEngine : IBacktestEngine
{
    private readonly ILogger<BacktestEngine> _logger;
    private readonly HistoricalSimulator _historicalSimulator;
    private readonly MonteCarloSimulator _monteCarloSimulator;
    private readonly WalkForwardAnalyzer _walkForwardAnalyzer;

    public BacktestEngine(
        ILogger<BacktestEngine> logger,
        HistoricalSimulator historicalSimulator,
        MonteCarloSimulator monteCarloSimulator,
        WalkForwardAnalyzer walkForwardAnalyzer)
    {
        _logger = logger;
        _historicalSimulator = historicalSimulator;
        _monteCarloSimulator = monteCarloSimulator;
        _walkForwardAnalyzer = walkForwardAnalyzer;
    }

    public async Task<BacktestResult> RunHistoricalBacktestAsync(
        HistoricalData historicalData,
        BacktestConfiguration config,
        Func<HistoricalBar, decimal, Task<(int quantity, decimal entryPrice, bool shouldEnter)>> entryLogic,
        Func<HistoricalBar, decimal, Task<(decimal exitPrice, bool shouldExit)>> exitLogic)
    {
        _logger.LogInformation("Running historical backtest for {symbol} from {start} to {end}",
            historicalData.Symbol, config.StartDate, config.EndDate);

        return await Task.Run(() =>
            _historicalSimulator.SimulateHistorical(historicalData, config, entryLogic, exitLogic));
    }

    public async Task<List<BacktestResult>> RunMonteCarloAsync(
        List<TradeExecution> trades,
        BacktestConfiguration config,
        decimal dailyVolatility)
    {
        _logger.LogInformation("Running Monte Carlo simulation with {sims} iterations",
            config.NumberOfSimulations);

        return await Task.Run(() =>
            _monteCarloSimulator.SimulateMonteCarlo(trades, config, dailyVolatility));
    }

    public async Task<WalkForwardResult> RunWalkForwardAsync(
        HistoricalData historicalData,
        BacktestConfiguration config,
        Func<HistoricalData, BacktestConfiguration, Task<BacktestResult>> optimizeAndTest)
    {
        _logger.LogInformation("Running walk-forward analysis for {symbol}",
            historicalData.Symbol);

        return await Task.Run(async () =>
            _walkForwardAnalyzer.AnalyzeWalkForward(historicalData, config, optimizeAndTest));
    }
}
