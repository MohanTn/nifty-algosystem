namespace NiftyOptionsAlgo.Engine;
using NiftyOptionsAlgo.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class StrategyEngine : IStrategyEngine
{
    private readonly StrategyConfig _config;
    private readonly IEventCalendar _eventCalendar;
    private readonly IGreeksCalculator _greeksCalculator;

    public StrategyEngine(StrategyConfig config, IEventCalendar eventCalendar, IGreeksCalculator greeksCalculator)
    {
        _config = config;
        _eventCalendar = eventCalendar;
        _greeksCalculator = greeksCalculator;
    }

    public async Task<EntryEvaluationResult> EvaluateEntryAsync()
    {
        var result = new EntryEvaluationResult { FailedChecks = new List<string>().ToArray() };
        var failures = new List<string>();

        // Rule 1: Time check (10-11 AM IST)
        var istTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
        if (istTime.Hour < _config.EntryWindowStartHour || istTime.Hour >= _config.EntryWindowEndHour)
        {
            failures.Add($"Rule 1: Current time {istTime:HH:mm} outside entry window {_config.EntryWindowStartHour}:00-{_config.EntryWindowEndHour}:00");
        }

        // Rule 2: VIX regime check (must be 14-18 for S1)
        // Mock VIX = 16 for now
        decimal mockVix = 16m;
        var regime = DetermineVixRegime(mockVix);
        if (regime != VixRegime.SweetSpot)
        {
            failures.Add($"Rule 2: VIX regime {regime} not in SweetSpot (14-18)");
        }

        // Rule 3: VIX direction check (must be falling/stable)
        // Assume falling for now
        var direction = VixDirection.Falling;
        if (direction == VixDirection.Rising)
        {
            failures.Add("Rule 3: VIX direction is rising, not falling/stable");
        }

        // Rule 4: DTE check (≥45 days)
        var nextExpiry = DateTime.Now.AddDays(50);
        int dte = (int)(nextExpiry - DateTime.Now).TotalDays;
        if (dte < _config.MinDteForEntry)
        {
            failures.Add($"Rule 4: DTE {dte} days less than minimum {_config.MinDteForEntry}");
        }

        // Rule 5: Expiry type (monthly, not weekly)
        bool isMonthlyExpiry = nextExpiry.Day > 20; // Simplified: monthly expiry after 20th
        if (!isMonthlyExpiry)
        {
            failures.Add("Rule 5: Target expiry is weekly, not monthly");
        }

        // Rule 6: Event check (no events within 5 days)
        bool isSafeFromEvents = await _eventCalendar.IsSafeToEnterAsync(DateTime.Now, nextExpiry);
        if (!isSafeFromEvents)
        {
            failures.Add("Rule 6: Market events within 5-day window");
        }

        // Rule 7: Capital deployment check (<70%)
        decimal deployedPercent = 0.40m; // Mock: assume 40% deployed
        if (deployedPercent > _config.MaxDeployedPercent)
        {
            failures.Add($"Rule 7: Deployed capital {deployedPercent:P} exceeds {_config.MaxDeployedPercent:P}");
        }

        // Rule 8: Cooling period check (sufficient days since last trade)
        // Mock: assume cooling period satisfied
        bool coolingPeriodOk = true;
        if (!coolingPeriodOk)
        {
            failures.Add("Rule 8: Cooling period not satisfied");
        }

        // Rule 9: No existing S1 position
        bool hasExistingS1 = false; // Mock: assume no existing position
        if (hasExistingS1)
        {
            failures.Add("Rule 9: Existing S1 position already open");
        }

        // Rules 10-12: Strike selection and lot sizing
        if (failures.Count == 0)
        {
            decimal niftySpot = 23500m; // Mock spot price

            // Rule 10: Find 20-delta strikes
            int peStrike = FindStrike(niftySpot, -0.20m);
            int ceStrike = FindStrike(niftySpot, 0.20m);

            // Mock premium calculation
            decimal pePremium = 150m;
            decimal cePremium = 145m;
            decimal totalPremium = (pePremium + cePremium) * _config.NiftyLotSize;

            // Rule 11: Lot size calculation
            decimal availableCapital = _config.CapitalTotal * (1 - _config.ReservePercent);
            decimal maxLossPerTrade = _config.CapitalTotal * _config.MaxRiskPerTradePercent;
            int recommendedLots = (int)(maxLossPerTrade / totalPremium);

            // Rule 12: Minimum lot size
            if (recommendedLots >= 1)
            {
                result.ShouldEnter = failures.Count == 0;
                result.RecommendedStrategy = StrategyType.S1_Strangle;
                result.RecommendedLots = recommendedLots;
                result.PeStrike = peStrike;
                result.CeStrike = ceStrike;
                result.PePremium = pePremium;
                result.CePremium = cePremium;
                result.TotalPremium = totalPremium;
                result.PeGttTrigger = pePremium * (decimal)_config.GttMultiplier;
                result.CeGttTrigger = cePremium * (decimal)_config.GttMultiplier;
                result.TargetExpiry = nextExpiry;
            }
            else
            {
                failures.Add("Rule 12: Calculated lot size < 1");
            }
        }

        result.FailedChecks = failures.ToArray();
        return result;
    }

    public async Task<bool> ShouldExitAsync(StrangleTrade trade)
    {
        // Check profit target (50% of entry premium)
        decimal currentPremium = 100m; // Mock current premium
        decimal targetPremium = trade.TotalPremiumCollected * (1 - (decimal)_config.ProfitTargetPercent);

        if (currentPremium <= targetPremium)
            return true; // Exit: profit target hit

        // Check stop loss (2% of capital)
        decimal maxLoss = _config.CapitalTotal * _config.MaxLossPercent;
        if (trade.UnrealizedPnl <= -maxLoss)
            return true; // Exit: stop loss hit

        // Check DTE (≤21)
        int currentDte = (int)(trade.ExpiryDate - DateTime.Now).TotalDays;
        if (currentDte <= 21)
            return true; // Exit: DTE warning

        return false;
    }

    public async Task<StrategyType> DetermineStrategyAsync(decimal vix, VixDirection direction)
    {
        var regime = DetermineVixRegime(vix);
        return regime switch
        {
            VixRegime.SweetSpot => StrategyType.S1_Strangle,
            VixRegime.Elevated => StrategyType.S2_IronCondor,
            VixRegime.HighRisk => StrategyType.S3_Butterfly,
            _ => StrategyType.S4_Calendar
        };
    }

    private VixRegime DetermineVixRegime(decimal vix)
    {
        return vix switch
        {
            < 12m => VixRegime.DangerZone,
            < 14m => VixRegime.Caution,
            < 18m => VixRegime.SweetSpot,
            < 22m => VixRegime.Elevated,
            < 28m => VixRegime.HighRisk,
            _ => VixRegime.Crisis
        };
    }

    private int FindStrike(decimal spot, decimal targetDelta)
    {
        // Simplified strike finder: find strike closest to target delta
        // In real implementation, would use Greeks calculator
        int baseStrike = (int)(spot / 100) * 100;
        if (targetDelta > 0)
            return baseStrike + 500; // OTM call
        else
            return baseStrike - 500; // OTM put
    }

}
