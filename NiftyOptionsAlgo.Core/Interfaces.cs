namespace NiftyOptionsAlgo.Core;

public interface IGreeksCalculator
{
    Greeks Calculate(decimal spotPrice, decimal strikePrice, decimal riskFreeRate, decimal impliedVolatility, decimal daysToExpiry, OptionType optionType);
    decimal ComputeImpliedVolatility(decimal marketPrice, decimal spotPrice, decimal strikePrice, decimal riskFreeRate, decimal daysToExpiry, OptionType optionType);
    decimal ComputePortfolioDelta(List<TradeLeg> legs, decimal spotPrice);
}

public interface IStrategyEngine
{
    Task<EntryEvaluationResult> EvaluateEntryAsync();
    Task<bool> ShouldExitAsync(StrangleTrade trade);
    Task<StrategyType> DetermineStrategyAsync(decimal vix, VixDirection direction);
}

public interface IPositionMonitor
{
    Task MonitorAllPositionsAsync();
    Task<PositionStatus> GetPositionStatusAsync(Guid tradeId);
}

public interface IOrderExecutor
{
    Task<OrderResult> EnterStrangleAsync(EntryEvaluationResult plan);
    Task<OrderResult> ExitPositionAsync(Guid tradeId, ExitReason reason);
}

public interface IRiskManager
{
    Task<RiskAssessment> AssessPortfolioRiskAsync();
    Task<bool> IsTradeAllowedAsync(EntryEvaluationResult plan);
    Task EmergencyExitAllAsync(string reason);
}

public interface IEventCalendar
{
    Task<List<MarketEvent>> GetUpcomingEventsAsync(int days);
    Task<bool> IsEventWithinWindowAsync(DateTime date, int windowDays);
    Task<bool> IsSafeToEnterAsync(DateTime entryDate, DateTime expiryDate);
}

public interface IKiteService
{
    Task<bool> IsSessionValidAsync();
    Task<Quote> GetQuoteAsync(string tradingSymbol);
    Task<List<Quote>> GetQuotesAsync(List<string> tradingSymbols);
    Task<int> PlaceOrderAsync(OrderRequest request);
    Task<bool> CancelOrderAsync(int orderId);
    Task<List<Position>> GetPositionsAsync();
}

public interface ITickerService
{
    Task StartAsync(List<uint> instrumentTokens);
    Task StopAsync();
    event EventHandler<TickData> OnTick;
}

public interface INotificationService
{
    Task SendTradeEnteredAsync(StrangleTrade trade);
    Task SendTradeExitedAsync(StrangleTrade trade, ExitReason reason);
    Task SendEmergencyAlertAsync(string message);
}

public class Quote { public string Symbol { get; set; } = string.Empty; public decimal LastPrice { get; set; } }
public class OrderRequest { public string Symbol { get; set; } = string.Empty; public int Quantity { get; set; } public string OrderType { get; set; } = string.Empty; }
public class Order { public int OrderId { get; set; } public OrderRequest Request { get; set; } = new(); }
public class Position { public string Symbol { get; set; } = string.Empty; public int Quantity { get; set; } }
public class TickData { public string Symbol { get; set; } = string.Empty; public decimal Price { get; set; } }
public class PositionStatus { public Guid TradeId { get; set; } public string Status { get; set; } = string.Empty; }
public class OrderResult { public bool Success { get; set; } public string Message { get; set; } = string.Empty; }
public class RiskAssessment { public bool IsWithinLimits { get; set; } public decimal CurrentDrawdown { get; set; } }
