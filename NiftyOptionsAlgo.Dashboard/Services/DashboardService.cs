using NiftyOptionsAlgo.Core;

namespace NiftyOptionsAlgo.Dashboard.Services;

public class DashboardService : IDashboardService
{
    private readonly IPositionMonitor _positionMonitor;
    private readonly IRiskManager _riskManager;
    private readonly ILogger<DashboardService> _logger;
    private readonly List<StrangleTrade> _allTrades = new();
    private readonly List<StrangleTrade> _openPositions = new();

    public DashboardService(
        IPositionMonitor positionMonitor,
        IRiskManager riskManager,
        ILogger<DashboardService> logger)
    {
        _positionMonitor = positionMonitor;
        _riskManager = riskManager;
        _logger = logger;
    }

    public async Task<List<StrangleTrade>> GetOpenPositionsAsync()
    {
        _logger.LogDebug("Fetching open positions");
        return await Task.FromResult(_openPositions);
    }

    public async Task<List<StrangleTrade>> GetRecentTradesAsync(int limit)
    {
        _logger.LogDebug("Fetching {limit} recent trades", limit);
        return await Task.FromResult(_allTrades.TakeLast(limit).ToList());
    }

    public async Task<RiskMetricsDto> GetRiskMetricsAsync()
    {
        _logger.LogDebug("Calculating risk metrics");
        var assessment = await _riskManager.AssessPortfolioRiskAsync();

        var dto = new RiskMetricsDto
        {
            TotalDeployedCapital = _openPositions.Sum(t => t.TotalPremiumCollected),
            CurrentDrawdown = assessment.CurrentDrawdown,
            DrawdownPercent = assessment.CurrentDrawdown,
            IsWithinLimits = assessment.IsWithinLimits,
            ActivePositions = _openPositions.Count,
            UnrealizedPnL = _openPositions.Sum(t => t.UnrealizedPnl),
            RealizedPnL = _allTrades.Sum(t => t.RealizedPnl)
        };

        dto.PortfolioValue = dto.TotalDeployedCapital + dto.UnrealizedPnL;
        dto.AvailableCapital = 1000000m - dto.TotalDeployedCapital;

        return dto;
    }

    public async Task<PortfolioSummaryDto> GetPortfolioSummaryAsync()
    {
        _logger.LogDebug("Building portfolio summary");
        var winningTrades = _allTrades.Count(t => t.RealizedPnl > 0);

        var summary = new PortfolioSummaryDto
        {
            InitialCapital = 1000000m,
            CurrentCapital = 1000000m + _allTrades.Sum(t => t.RealizedPnl),
            TotalReturn = _allTrades.Sum(t => t.RealizedPnl),
            TotalTrades = _allTrades.Count,
            WinningTrades = winningTrades,
            WinRate = _allTrades.Count > 0 ? (decimal)winningTrades / _allTrades.Count : 0,
            MaxDrawdown = 0.05m
        };

        summary.ReturnPercent = summary.TotalReturn / summary.InitialCapital;

        return await Task.FromResult(summary);
    }

    public void AddOpenPosition(StrangleTrade trade)
    {
        _openPositions.Add(trade);
        _allTrades.Add(trade);
        _logger.LogInformation("Added open position: {tradeId}", trade.Id);
    }

    public void RemoveOpenPosition(Guid tradeId)
    {
        var trade = _openPositions.FirstOrDefault(t => t.Id == tradeId);
        if (trade != null)
        {
            _openPositions.Remove(trade);
            _logger.LogInformation("Closed position: {tradeId}", tradeId);
        }
    }

    public void UpdatePosition(StrangleTrade trade)
    {
        var idx = _openPositions.FindIndex(t => t.Id == trade.Id);
        if (idx >= 0)
        {
            _openPositions[idx] = trade;
            _logger.LogDebug("Updated position: {tradeId}", trade.Id);
        }
    }
}
