namespace NiftyOptionsAlgo.RiskManager;
using NiftyOptionsAlgo.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class RiskManager : IRiskManager
{
    private readonly StrategyConfig _config;
    private readonly Dictionary<Guid, StrangleTrade> _trades = new();
    private decimal _dailyLoss = 0;
    private decimal _weeklyLoss = 0;
    private decimal _monthlyLoss = 0;

    public RiskManager(StrategyConfig config)
    {
        _config = config;
    }

    public async Task<RiskAssessment> AssessPortfolioRiskAsync()
    {
        decimal totalDeployed = 0;
        decimal currentDrawdown = 0;

        foreach (var trade in _trades.Values)
        {
            if (trade.Status == TradeStatus.Open)
            {
                totalDeployed += trade.TotalPremiumCollected;
                currentDrawdown += trade.UnrealizedPnl < 0 ? Math.Abs(trade.UnrealizedPnl) : 0;
            }
        }

        decimal deployedPercent = totalDeployed / _config.CapitalTotal;

        return new RiskAssessment
        {
            IsWithinLimits = deployedPercent <= _config.MaxDeployedPercent && currentDrawdown <= _config.CapitalTotal * _config.MaxLossPercent,
            CurrentDrawdown = currentDrawdown
        };
    }

    public async Task<bool> IsTradeAllowedAsync(EntryEvaluationResult plan)
    {
        var assessment = await AssessPortfolioRiskAsync();
        if (!assessment.IsWithinLimits)
            return false;

        // Rule 1: Deployed < 70%
        decimal deployedPercent = (plan.TotalPremium * plan.RecommendedLots) / _config.CapitalTotal;
        if (deployedPercent > _config.MaxDeployedPercent)
            return false;

        // Rule 3: Daily loss limit (5%)
        if (_dailyLoss > _config.CapitalTotal * 0.05m)
            return false;

        // Rule 4: Weekly loss limit (8%)
        if (_weeklyLoss > _config.CapitalTotal * 0.08m)
            return false;

        // Rule 7: Monthly loss limit (12%)
        if (_monthlyLoss > _config.CapitalTotal * 0.12m)
            return false;

        return true;
    }

    public async Task EmergencyExitAllAsync(string reason)
    {
        // VIX > 28: emergency exit all positions
        foreach (var trade in _trades.Values)
        {
            if (trade.Status == TradeStatus.Open)
            {
                trade.Status = TradeStatus.Closed;
                trade.ExitReason = ExitReason.EventRisk;
                trade.ExitDate = DateTime.Now;
            }
        }
    }

    // Hard Rule 2: Single trade loss ≤ 2%
    public bool ValidateSingleTradeLoss(decimal loss)
    {
        decimal maxLoss = _config.CapitalTotal * _config.MaxLossPercent;
        return Math.Abs(loss) <= maxLoss;
    }

    // Hard Rule 5: VIX > 28 emergency exit
    public async Task CheckVixEmergencyAsync(decimal vix)
    {
        if (vix > _config.VixCrisisThreshold)
        {
            await EmergencyExitAllAsync("VIX emergency");
        }
    }

    // Hard Rule 6: Margin sufficiency
    public async Task<bool> IsMarginSufficientAsync(decimal requiredMargin)
    {
        // Mock: check if margin > 30% of required
        decimal availableMargin = _config.CapitalTotal * 0.40m; // Mock margin
        return availableMargin > requiredMargin * 0.30m;
    }

    // Hard Rule 8: 3 consecutive losses → reduce size 50%
    private int _consecutiveLosses = 0;
    private decimal _currentLotSizeMultiplier = 1.0m;

    public void UpdateConsecutiveLosses(bool tradeLost)
    {
        if (tradeLost)
        {
            _consecutiveLosses++;
            if (_consecutiveLosses >= 3)
            {
                _currentLotSizeMultiplier = 0.5m;
                _consecutiveLosses = 0;
            }
        }
        else
        {
            _consecutiveLosses = 0;
        }
    }

    public decimal GetLotSizeMultiplier() => _currentLotSizeMultiplier;
}
