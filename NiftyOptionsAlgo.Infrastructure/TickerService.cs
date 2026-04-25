namespace NiftyOptionsAlgo.Infrastructure;
using NiftyOptionsAlgo.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TickerService : ITickerService
{
    public event EventHandler<TickData> OnTick;
    public event EventHandler OnConnected;
    public event EventHandler OnDisconnected;
    public event EventHandler<string> OnError;

    private bool _isRunning = false;

    public async Task StartAsync(List<uint> instrumentTokens)
    {
        _isRunning = true;
        OnConnected?.Invoke(this, EventArgs.Empty);

        // Simulate ticker updates
        while (_isRunning)
        {
            await Task.Delay(1000);
            OnTick?.Invoke(this, new TickData { Symbol = "NIFTY", Price = 23500m + new Random().Next(-100, 100) });
        }
    }

    public async Task StopAsync()
    {
        _isRunning = false;
        OnDisconnected?.Invoke(this, EventArgs.Empty);
    }
}
