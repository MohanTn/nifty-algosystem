namespace NiftyOptionsAlgo.Backtester;

public class PerformanceCalculator
{
    public static PerformanceMetrics CalculateMetrics(
        List<TradeExecution> trades,
        decimal initialCapital,
        decimal finalCapital,
        List<decimal> dailyReturns,
        int tradingDaysPerYear = 252)
    {
        var metrics = new PerformanceMetrics();

        if (trades.Count == 0)
            return metrics;

        // Basic metrics
        metrics.TotalReturn = finalCapital - initialCapital;
        var totalReturnPercent = (finalCapital - initialCapital) / initialCapital;
        metrics.AnnualizedReturn = CalculateAnnualizedReturn(totalReturnPercent,
            (trades.Last().ExitDate - trades.First().ExecutionDate).TotalDays);

        // Risk metrics
        metrics.AnnualizedVolatility = CalculateAnnualizedVolatility(dailyReturns, tradingDaysPerYear);
        metrics.Sharpe = CalculateSharpeRatio(dailyReturns, 0.04m, tradingDaysPerYear);
        metrics.Sortino = CalculateSortinoRatio(dailyReturns, 0.04m, tradingDaysPerYear);

        // Drawdown metrics
        var (maxDD, maxDDDuration) = CalculateMaxDrawdown(trades, initialCapital);
        metrics.MaxDrawdown = maxDD;
        metrics.MaxDrawdownDuration = maxDDDuration;
        metrics.Calmar = metrics.MaxDrawdown > 0 ? metrics.AnnualizedReturn / Math.Abs(metrics.MaxDrawdown) : 0;

        // Win rate metrics
        var winningTrades = trades.Where(t => t.PnL > 0).ToList();
        var losingTrades = trades.Where(t => t.PnL <= 0).ToList();
        metrics.WinRate = trades.Count > 0 ? (decimal)winningTrades.Count / trades.Count : 0;
        metrics.ProfitFactor = losingTrades.Count > 0
            ? winningTrades.Sum(t => t.PnL) / Math.Abs(losingTrades.Sum(t => t.PnL))
            : 0;

        // Consecutive metrics
        metrics.ConsecutiveWins = CalculateConsecutiveWins(trades);
        metrics.ConsecutiveLosses = CalculateConsecutiveLosses(trades);

        // Recovery factor
        var totalLoss = Math.Abs(losingTrades.Sum(t => t.PnL));
        metrics.RecoveryFactor = totalLoss > 0 ? metrics.TotalReturn / totalLoss : 0;

        return metrics;
    }

    private static decimal CalculateAnnualizedReturn(decimal totalReturn, double daysTraded)
    {
        if (daysTraded <= 0) return 0;
        var yearsTraded = daysTraded / 365.0;
        return (decimal)Math.Pow((double)(1 + totalReturn), 1.0 / yearsTraded) - 1;
    }

    private static decimal CalculateAnnualizedVolatility(List<decimal> dailyReturns, int tradingDaysPerYear)
    {
        if (dailyReturns.Count < 2) return 0;

        var mean = dailyReturns.Average();
        var variance = dailyReturns.Sum(r => (r - mean) * (r - mean)) / (dailyReturns.Count - 1);
        var stdDev = (decimal)Math.Sqrt((double)variance);

        return stdDev * (decimal)Math.Sqrt(tradingDaysPerYear);
    }

    private static decimal CalculateSharpeRatio(List<decimal> dailyReturns, decimal riskFreeRate, int tradingDaysPerYear)
    {
        if (dailyReturns.Count == 0) return 0;

        var avgReturn = dailyReturns.Average();
        var dailyRiskFreeRate = riskFreeRate / tradingDaysPerYear;
        var excessReturn = avgReturn - dailyRiskFreeRate;
        var volatility = CalculateAnnualizedVolatility(dailyReturns, tradingDaysPerYear);

        return volatility > 0 ? (excessReturn * tradingDaysPerYear) / volatility : 0;
    }

    private static decimal CalculateSortinoRatio(List<decimal> dailyReturns, decimal riskFreeRate, int tradingDaysPerYear)
    {
        if (dailyReturns.Count < 2) return 0;

        var avgReturn = dailyReturns.Average();
        var dailyRiskFreeRate = riskFreeRate / tradingDaysPerYear;
        var excessReturn = avgReturn - dailyRiskFreeRate;

        var downVariance = dailyReturns
            .Where(r => r < dailyRiskFreeRate)
            .Sum(r => (r - dailyRiskFreeRate) * (r - dailyRiskFreeRate)) / dailyReturns.Count;
        var downStdDev = (decimal)Math.Sqrt((double)downVariance);
        var annualizedDownVolatility = downStdDev * (decimal)Math.Sqrt(tradingDaysPerYear);

        return annualizedDownVolatility > 0 ? (excessReturn * tradingDaysPerYear) / annualizedDownVolatility : 0;
    }

    private static (decimal, decimal) CalculateMaxDrawdown(List<TradeExecution> trades, decimal initialCapital)
    {
        if (trades.Count == 0) return (0, 0);

        var runningCapital = initialCapital;
        var peakCapital = initialCapital;
        var maxDD = decimal.Zero;
        var maxDDStartDate = trades.First().ExecutionDate;
        var maxDDEndDate = trades.First().ExecutionDate;
        var currentDDStartDate = trades.First().ExecutionDate;

        foreach (var trade in trades)
        {
            runningCapital += trade.PnL;
            if (runningCapital > peakCapital)
            {
                peakCapital = runningCapital;
                currentDDStartDate = trade.ExitDate;
            }

            var dd = (peakCapital - runningCapital) / peakCapital;
            if (dd > maxDD)
            {
                maxDD = dd;
                maxDDStartDate = currentDDStartDate;
                maxDDEndDate = trade.ExitDate;
            }
        }

        var duration = (maxDDEndDate - maxDDStartDate).TotalDays;
        return (-maxDD, (decimal)duration);
    }

    private static int CalculateConsecutiveWins(List<TradeExecution> trades)
    {
        var maxWins = 0;
        var currentWins = 0;

        foreach (var trade in trades)
        {
            if (trade.PnL > 0)
            {
                currentWins++;
                maxWins = Math.Max(maxWins, currentWins);
            }
            else
            {
                currentWins = 0;
            }
        }

        return maxWins;
    }

    private static int CalculateConsecutiveLosses(List<TradeExecution> trades)
    {
        var maxLosses = 0;
        var currentLosses = 0;

        foreach (var trade in trades)
        {
            if (trade.PnL <= 0)
            {
                currentLosses++;
                maxLosses = Math.Max(maxLosses, currentLosses);
            }
            else
            {
                currentLosses = 0;
            }
        }

        return maxLosses;
    }
}
