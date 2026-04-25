using NiftyOptionsAlgo.Core;

namespace NiftyOptionsAlgo.Dashboard.Services;

public interface IDashboardService
{
    Task<List<StrangleTrade>> GetOpenPositionsAsync();
    Task<List<StrangleTrade>> GetRecentTradesAsync(int limit);
    Task<RiskMetricsDto> GetRiskMetricsAsync();
    Task<PortfolioSummaryDto> GetPortfolioSummaryAsync();
}

public class RiskMetricsDto
{
    public decimal TotalDeployedCapital { get; set; }
    public decimal AvailableCapital { get; set; }
    public decimal CurrentDrawdown { get; set; }
    public decimal DrawdownPercent { get; set; }
    public bool IsWithinLimits { get; set; }
    public int ActivePositions { get; set; }
    public decimal PortfolioValue { get; set; }
    public decimal UnrealizedPnL { get; set; }
    public decimal RealizedPnL { get; set; }
}

public class PortfolioSummaryDto
{
    public decimal InitialCapital { get; set; }
    public decimal CurrentCapital { get; set; }
    public decimal TotalReturn { get; set; }
    public decimal ReturnPercent { get; set; }
    public int TotalTrades { get; set; }
    public int WinningTrades { get; set; }
    public decimal WinRate { get; set; }
    public decimal Sharpe { get; set; }
    public decimal MaxDrawdown { get; set; }
}
