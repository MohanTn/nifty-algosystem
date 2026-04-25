namespace NiftyOptionsAlgo.Core;

using Microsoft.Extensions.Logging;

public interface ILiveTradingSafetyGate
{
    bool IsLiveTradingEnabled { get; }
    void EnableLiveTrading(string authToken);
    void DisableLiveTrading();
    bool ValidateBeforeTrade(OrderRequest request);
}

public class LiveTradingSafetyGate : ILiveTradingSafetyGate
{
    private bool _isEnabled = false;
    private string _authToken = string.Empty;
    private DateTime _enabledAt = DateTime.MinValue;
    private readonly int _maxSessionDurationMinutes = 480; // 8 hours
    private readonly ILogger<LiveTradingSafetyGate> _logger;

    public bool IsLiveTradingEnabled
    {
        get
        {
            if (!_isEnabled) return false;

            // Auto-disable after max session duration
            if ((DateTime.UtcNow - _enabledAt).TotalMinutes > _maxSessionDurationMinutes)
            {
                DisableLiveTrading();
                return false;
            }

            return _isEnabled;
        }
    }

    public LiveTradingSafetyGate(ILogger<LiveTradingSafetyGate> logger)
    {
        _logger = logger;
    }

    public void EnableLiveTrading(string authToken)
    {
        if (string.IsNullOrEmpty(authToken))
            throw new InvalidOperationException("Auth token required to enable live trading");

        _authToken = authToken;
        _isEnabled = true;
        _enabledAt = DateTime.UtcNow;

        _logger.LogWarning("CRITICAL: Live trading ENABLED at {time}", DateTime.UtcNow);
    }

    public void DisableLiveTrading()
    {
        _isEnabled = false;
        _authToken = string.Empty;

        _logger.LogWarning("CRITICAL: Live trading DISABLED at {time}", DateTime.UtcNow);
    }

    public bool ValidateBeforeTrade(OrderRequest request)
    {
        if (!IsLiveTradingEnabled)
        {
            _logger.LogError("Trade blocked: Live trading not enabled");
            return false;
        }

        if (string.IsNullOrEmpty(request.Symbol))
        {
            _logger.LogError("Trade blocked: Invalid symbol");
            return false;
        }

        if (request.Quantity <= 0)
        {
            _logger.LogError("Trade blocked: Invalid quantity");
            return false;
        }

        _logger.LogInformation("Trade validated: {symbol} {qty} {type}",
            request.Symbol, request.Quantity, request.OrderType);

        return true;
    }
}
