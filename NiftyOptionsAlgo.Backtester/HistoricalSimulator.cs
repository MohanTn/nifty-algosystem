namespace NiftyOptionsAlgo.Backtester;

using Microsoft.Extensions.Logging;

public class HistoricalSimulator
{
    private readonly ILogger<HistoricalSimulator> _logger;

    public HistoricalSimulator(ILogger<HistoricalSimulator> logger)
    {
        _logger = logger;
    }

    public BacktestResult SimulateHistorical(
        HistoricalData historicalData,
        BacktestConfiguration config,
        Func<HistoricalBar, decimal, Task<(int quantity, decimal entryPrice, bool shouldEnter)>> entryLogic,
        Func<HistoricalBar, decimal, Task<(decimal exitPrice, bool shouldExit)>> exitLogic)
    {
        _logger.LogInformation("Starting historical simulation from {start} to {end}",
            config.StartDate, config.EndDate);

        var result = new BacktestResult
        {
            RunDate = DateTime.UtcNow,
            InitialCapital = config.InitialCapital,
            FinalCapital = config.InitialCapital
        };

        var capital = config.InitialCapital;
        var openTrades = new List<(decimal entryPrice, int quantity, DateTime entryDate, string symbol)>();

        foreach (var bar in historicalData.Bars)
        {
            if (bar.Timestamp < config.StartDate || bar.Timestamp > config.EndDate)
                continue;

            // Check exit conditions
            var closedTrades = new List<int>();
            for (int i = openTrades.Count - 1; i >= 0; i--)
            {
                var (exitPrice, shouldExit) = exitLogic(bar, openTrades[i].entryPrice).Result;

                if (shouldExit)
                {
                    var trade = openTrades[i];
                    var pnl = (exitPrice - trade.entryPrice) * trade.quantity;
                    if (config.IncludeTransactionCosts)
                    {
                        pnl -= pnl * config.TransactionCostPercent;
                    }

                    capital += pnl;
                    result.Trades.Add(new TradeExecution
                    {
                        ExecutionDate = trade.entryDate,
                        Symbol = trade.symbol,
                        EntryPrice = trade.entryPrice,
                        ExitPrice = exitPrice,
                        ExitDate = bar.Timestamp,
                        Quantity = trade.quantity,
                        PnL = pnl,
                        PnLPercent = pnl / (trade.entryPrice * trade.quantity)
                    });

                    closedTrades.Add(i);
                }
            }

            foreach (var idx in closedTrades.OrderByDescending(x => x))
            {
                openTrades.RemoveAt(idx);
            }

            // Check entry conditions
            var (qty, entryPrice, shouldEnter) = entryLogic(bar, capital).Result;
            if (shouldEnter && qty > 0 && capital > entryPrice * qty)
            {
                openTrades.Add((entryPrice, qty, bar.Timestamp, historicalData.Symbol));
                capital -= entryPrice * qty;
                if (config.IncludeTransactionCosts)
                {
                    capital -= (entryPrice * qty) * config.TransactionCostPercent;
                }
            }
        }

        // Close remaining open trades at last bar
        if (historicalData.Bars.Count > 0)
        {
            var lastBar = historicalData.Bars.Last();
            foreach (var (entryPrice, qty, entryDate, symbol) in openTrades)
            {
                var pnl = (lastBar.Close - entryPrice) * qty;
                capital += pnl;
                result.Trades.Add(new TradeExecution
                {
                    ExecutionDate = entryDate,
                    Symbol = symbol,
                    EntryPrice = entryPrice,
                    ExitPrice = lastBar.Close,
                    ExitDate = lastBar.Timestamp,
                    Quantity = qty,
                    PnL = pnl,
                    PnLPercent = pnl / (entryPrice * qty)
                });
            }
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
            var winSum = result.Trades.Where(t => t.PnL > 0).Sum(t => t.PnL);
            var lossSum = result.Trades.Where(t => t.PnL <= 0).Sum(t => t.PnL);
            result.AverageWin = result.WinningTrades > 0 ? winSum / result.WinningTrades : 0;
            result.AverageLoss = result.LosingTrades > 0 ? lossSum / result.LosingTrades : 0;
            result.ProfitFactor = Math.Abs(lossSum) > 0 ? winSum / Math.Abs(lossSum) : 0;
            result.ExpectancyPerTrade = result.TotalReturn / result.TotalTrades;
        }

        _logger.LogInformation("Historical simulation complete: {trades} trades, {return:P2} return",
            result.TotalTrades, result.TotalReturnPercent);

        return result;
    }
}
