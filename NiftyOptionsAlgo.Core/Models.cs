namespace NiftyOptionsAlgo.Core;

public class StrategyConfig
{
    public decimal VixDangerZone { get; set; } = 12.0m;
    public decimal VixCautionLow { get; set; } = 14.0m;
    public decimal VixSweetSpotLow { get; set; } = 14.0m;
    public decimal VixSweetSpotHigh { get; set; } = 18.0m;
    public decimal VixElevatedHigh { get; set; } = 22.0m;
    public decimal VixCrisisThreshold { get; set; } = 28.0m;
    public decimal CapitalTotal { get; set; } = 1000000m;
    public decimal MaxRiskPerTradePercent { get; set; } = 0.02m;
    public decimal ReservePercent { get; set; } = 0.30m;
    public int NiftyLotSize { get; set; } = 25;
    public int MinDteForEntry { get; set; } = 45;
    public int EntryWindowStartHour { get; set; } = 10;
    public int EntryWindowEndHour { get; set; } = 11;
    public decimal TargetDeltaStrike { get; set; } = 0.20m;
    public decimal DeltaTolerance { get; set; } = 0.02m;
    public decimal GttMultiplier { get; set; } = 2.0m;
    public decimal GttWarningPercent { get; set; } = 0.80m;
    public decimal ProfitTargetPercent { get; set; } = 0.50m;
    public decimal MaxLossPercent { get; set; } = 0.02m;
    public int MaxAdjustments { get; set; } = 2;
    public int MinDteForAdjustment { get; set; } = 21;
    public decimal AdjustmentDeltaThreshold { get; set; } = 0.20m;
    public decimal AdjustmentPnlThreshold { get; set; } = 0.50m;
    public decimal Strategy1BucketPercent { get; set; } = 0.40m;
    public decimal Strategy2BucketPercent { get; set; } = 0.20m;
    public decimal Strategy3BucketPercent { get; set; } = 0.05m;
    public decimal Strategy4BucketPercent { get; set; } = 0.05m;
    public decimal MaxDeployedPercent { get; set; } = 0.70m;
    public int CoolingPeriodProfitExit { get; set; } = 1;
    public int CoolingPeriodSingleGtt { get; set; } = 5;
    public int CoolingPeriodDoubleGtt { get; set; } = 15;
}

public class StrangleTrade
{
    public Guid Id { get; set; }
    public DateTime EntryDate { get; set; }
    public DateTime? ExitDate { get; set; }
    public TradeStatus Status { get; set; }
    public StrategyType Strategy { get; set; }
    public int Lots { get; set; }
    public decimal NiftySpotAtEntry { get; set; }
    public decimal VixAtEntry { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int DteAtEntry { get; set; }
    public decimal TotalPremiumCollected { get; set; }
    public decimal ProfitTarget { get; set; }
    public decimal StopLossAmount { get; set; }
    public decimal RealizedPnl { get; set; }
    public decimal UnrealizedPnl { get; set; }
    public int AdjustmentCount { get; set; }
    public ExitReason? ExitReason { get; set; }
    public List<TradeLeg> Legs { get; set; } = new();
    public List<TradeAdjustment> Adjustments { get; set; } = new();
}

public class TradeLeg
{
    public Guid Id { get; set; }
    public Guid TradeId { get; set; }
    public LegType Type { get; set; }
    public string TradingSymbol { get; set; } = string.Empty;
    public int Strike { get; set; }
    public OptionType OptionType { get; set; }
    public int Quantity { get; set; }
    public decimal EntryPrice { get; set; }
    public decimal? ExitPrice { get; set; }
    public decimal EntryDelta { get; set; }
    public decimal GttTriggerPrice { get; set; }
    public int? ZerodhaGttOrderId { get; set; }
    public LegStatus Status { get; set; }
    public decimal UnrealizedPnl { get; set; }
    public decimal RealizedPnl { get; set; }
}

public class TradeAdjustment
{
    public Guid Id { get; set; }
    public Guid TradeId { get; set; }
    public DateTime AdjustmentDate { get; set; }
    public int ClosedStrike { get; set; }
    public int NewStrike { get; set; }
    public decimal ClosingPrice { get; set; }
    public decimal EntryPrice { get; set; }
}

public class VixSnapshot
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal Value { get; set; }
    public VixRegime Regime { get; set; }
    public VixDirection Direction { get; set; }
    public decimal FiveDayMA { get; set; }
}

public class MarketEvent
{
    public Guid Id { get; set; }
    public DateTime EventDate { get; set; }
    public string Name { get; set; } = string.Empty;
    public EventCategory Category { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public bool IsRecurring { get; set; }
    public string? RecurrencePattern { get; set; }
    public string? Notes { get; set; }
}

public class Greeks
{
    public decimal Delta { get; set; }
    public decimal Gamma { get; set; }
    public decimal Theta { get; set; }
    public decimal Vega { get; set; }
    public decimal Rho { get; set; }
}

public class EntryEvaluationResult
{
    public bool ShouldEnter { get; set; }
    public string[] FailedChecks { get; set; } = Array.Empty<string>();
    public StrategyType RecommendedStrategy { get; set; }
    public int RecommendedLots { get; set; }
    public int PeStrike { get; set; }
    public int CeStrike { get; set; }
    public decimal PePremium { get; set; }
    public decimal CePremium { get; set; }
    public decimal TotalPremium { get; set; }
    public decimal PeGttTrigger { get; set; }
    public decimal CeGttTrigger { get; set; }
    public DateTime TargetExpiry { get; set; }
}
