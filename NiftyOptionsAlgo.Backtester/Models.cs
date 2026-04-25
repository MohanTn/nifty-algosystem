namespace NiftyOptionsAlgo.Backtester;

public class HistoricalBar
{
    public DateTime Timestamp { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public long Volume { get; set; }
}

public class HistoricalData
{
    public string Symbol { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<HistoricalBar> Bars { get; set; } = new();
}

public class BacktestConfiguration
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal InitialCapital { get; set; } = 1000000m;
    public int NumberOfSimulations { get; set; } = 1000;
    public decimal ConfidenceLevel { get; set; } = 0.95m;
    public bool IncludeTransactionCosts { get; set; } = true;
    public decimal TransactionCostPercent { get; set; } = 0.001m;
    public int WalkForwardPeriodDays { get; set; } = 90;
    public int TestPeriodDays { get; set; } = 30;
}

public class TradeExecution
{
    public DateTime ExecutionDate { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public decimal EntryPrice { get; set; }
    public decimal ExitPrice { get; set; }
    public DateTime ExitDate { get; set; }
    public int Quantity { get; set; }
    public decimal Commission { get; set; }
    public decimal PnL { get; set; }
    public decimal PnLPercent { get; set; }
}

public class BacktestResult
{
    public DateTime RunDate { get; set; }
    public decimal InitialCapital { get; set; }
    public decimal FinalCapital { get; set; }
    public decimal TotalReturn { get; set; }
    public decimal TotalReturnPercent { get; set; }
    public decimal AnnualizedReturn { get; set; }
    public decimal Sharpe { get; set; }
    public decimal Sortino { get; set; }
    public decimal MaxDrawdown { get; set; }
    public decimal WinRate { get; set; }
    public decimal ProfitFactor { get; set; }
    public int TotalTrades { get; set; }
    public int WinningTrades { get; set; }
    public int LosingTrades { get; set; }
    public decimal AverageWin { get; set; }
    public decimal AverageLoss { get; set; }
    public decimal ExpectancyPerTrade { get; set; }
    public List<TradeExecution> Trades { get; set; } = new();
}

public class PerformanceMetrics
{
    public decimal TotalReturn { get; set; }
    public decimal AnnualizedReturn { get; set; }
    public decimal AnnualizedVolatility { get; set; }
    public decimal Sharpe { get; set; }
    public decimal Sortino { get; set; }
    public decimal Calmar { get; set; }
    public decimal MaxDrawdown { get; set; }
    public decimal MaxDrawdownDuration { get; set; }
    public decimal RecoveryFactor { get; set; }
    public decimal WinRate { get; set; }
    public decimal ProfitFactor { get; set; }
    public int ConsecutiveWins { get; set; }
    public int ConsecutiveLosses { get; set; }
}
