namespace NiftyOptionsAlgo.Infrastructure;

using System.Collections.Generic;

public class CircuitBreakerPolicy
{
    private int _failureCount = 0;
    private int _successCount = 0;
    private readonly int _failureThreshold = 5;
    private readonly int _successThreshold = 3;
    private CircuitState _state = CircuitState.Closed;
    private DateTime _lastFailureTime = DateTime.MinValue;
    private readonly int _resetTimeoutSeconds = 60;

    public CircuitState State => _state;

    public void RecordSuccess()
    {
        if (_state == CircuitState.Open)
        {
            var timeSinceLastFailure = DateTime.UtcNow - _lastFailureTime;
            if (timeSinceLastFailure.TotalSeconds >= _resetTimeoutSeconds)
            {
                _state = CircuitState.HalfOpen;
                _successCount = 0;
                _failureCount = 0;
            }
            else
            {
                throw new InvalidOperationException("Circuit breaker is open");
            }
        }

        if (_state == CircuitState.HalfOpen)
        {
            _successCount++;
            if (_successCount >= _successThreshold)
            {
                _state = CircuitState.Closed;
                _failureCount = 0;
                _successCount = 0;
            }
        }
    }

    public void RecordFailure()
    {
        _failureCount++;
        _lastFailureTime = DateTime.UtcNow;

        if (_failureCount >= _failureThreshold)
        {
            _state = CircuitState.Open;
        }
    }
}

public enum CircuitState
{
    Closed,
    Open,
    HalfOpen
}
