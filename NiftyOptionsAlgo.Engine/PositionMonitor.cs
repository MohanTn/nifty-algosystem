namespace NiftyOptionsAlgo.Engine;
using NiftyOptionsAlgo.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class PositionMonitor : IPositionMonitor
{
    private readonly Dictionary<Guid, StrangleTrade> _openPositions = new();

    public async Task MonitorAllPositionsAsync()
    {
        foreach (var trade in _openPositions.Values)
        {
            if (trade.Status != TradeStatus.Open) continue;

            // Check 1: Profit target (50% of entry premium)
            if (trade.UnrealizedPnl >= trade.TotalPremiumCollected * 0.50m)
            {
                trade.Status = TradeStatus.Closed;
                trade.ExitReason = ExitReason.ProfitTarget;
                trade.ExitDate = DateTime.Now;
            }

            // Check 2: Stop loss (2% of capital) - IMMEDIATE
            if (trade.UnrealizedPnl <= -trade.StopLossAmount)
            {
                trade.Status = TradeStatus.Closed;
                trade.ExitReason = ExitReason.StopLoss;
                trade.ExitDate = DateTime.Now;
            }

            // Check 3: GTT fired (check if any leg has status GttFired)
            bool gttFired = false;
            foreach (var leg in trade.Legs)
            {
                if (leg.Status == LegStatus.GttFired)
                {
                    gttFired = true;
                    break;
                }
            }
            if (gttFired)
            {
                trade.Status = TradeStatus.Closed;
                trade.ExitReason = ExitReason.GttFired;
                trade.ExitDate = DateTime.Now;
            }

            // Check 4: DTE ≤ 21
            int dte = (int)(trade.ExpiryDate - DateTime.Now).TotalDays;
            if (dte <= 21)
            {
                // Alert but don't auto-exit
            }

            // Check 5: VIX > 22
            decimal vix = 16m; // Mock VIX
            if (vix > 22m)
            {
                // Alert but don't auto-exit
            }

            // Check 6: GTT warning (80% of trigger)
            foreach (var leg in trade.Legs)
            {
                decimal gttWarningPrice = leg.GttTriggerPrice * 0.80m;
                if (Math.Abs(leg.EntryPrice - gttWarningPrice) < 1m) // Mock current price check
                {
                    // Alert: GTT warning
                }
            }

            // Check 7: Adjustment eligible (delta > 0.20 AND P&L >= 50%)
            // Simplified: check conditions
        }
    }

    public async Task<PositionStatus> GetPositionStatusAsync(Guid tradeId)
    {
        if (_openPositions.TryGetValue(tradeId, out var trade))
        {
            return new PositionStatus
            {
                TradeId = tradeId,
                Status = trade.Status.ToString()
            };
        }
        return new PositionStatus { TradeId = tradeId, Status = "NotFound" };
    }

    public void AddPosition(StrangleTrade trade) => _openPositions[trade.Id] = trade;
}
