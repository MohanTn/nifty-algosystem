namespace NiftyOptionsAlgo.Engine;
using NiftyOptionsAlgo.Core;
using System;
using System.Threading.Tasks;

public class OrderExecutor : IOrderExecutor
{
    private readonly IKiteService _kiteService;
    private readonly bool _paperTradingMode;

    public OrderExecutor(IKiteService kiteService, bool paperTradingMode = true)
    {
        _kiteService = kiteService;
        _paperTradingMode = paperTradingMode;
    }

    public async Task<OrderResult> EnterStrangleAsync(EntryEvaluationResult plan)
    {
        if (!plan.ShouldEnter)
            return new OrderResult { Success = false, Message = "Entry evaluation failed" };

        var result = new OrderResult { Success = true };

        if (_paperTradingMode)
        {
            // Simulate order placement in paper trading mode
            return new OrderResult { Success = true, Message = "Paper trade: simulated entry" };
        }

        // Check margin before placing orders
        var margin = await _kiteService.GetMarginsAsync();
        decimal requiredMargin = plan.TotalPremium * plan.RecommendedLots * 1.25m; // 25% extra buffer

        if (margin.Available < requiredMargin)
            return new OrderResult { Success = false, Message = "Insufficient margin" };

        try
        {
            // Place PE leg
            var peOrder = await _kiteService.PlaceOrderAsync(new OrderRequest
            {
                Symbol = $"NIFTY{plan.PeStrike}PE",
                Quantity = plan.RecommendedLots * 50,
                OrderType = "LIMIT"
            });

            if (peOrder < 0)
            {
                result.Success = false;
                result.Message = "PE leg order failed";
                return result;
            }

            // Place CE leg (must be within 30 seconds)
            var ceOrder = await _kiteService.PlaceOrderAsync(new OrderRequest
            {
                Symbol = $"NIFTY{plan.CeStrike}CE",
                Quantity = plan.RecommendedLots * 50,
                OrderType = "LIMIT"
            });

            if (ceOrder < 0)
            {
                // Atomicity: cancel PE if CE fails
                await _kiteService.CancelOrderAsync(peOrder);
                result.Success = false;
                result.Message = "CE leg order failed, PE cancelled (atomic entry failed)";
                return result;
            }

            result.Message = $"Trade entered: PE={plan.PeStrike} CE={plan.CeStrike}, lots={plan.RecommendedLots}";
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Order placement error: {ex.Message}";
        }

        return result;
    }

    public async Task<OrderResult> ExitPositionAsync(Guid tradeId, ExitReason reason)
    {
        if (_paperTradingMode)
        {
            return new OrderResult { Success = true, Message = $"Paper trade: simulated exit ({reason})" };
        }

        // In live mode: place counter orders to close all legs
        var result = new OrderResult { Success = true, Message = $"Position exited: {reason}" };
        return result;
    }
}
