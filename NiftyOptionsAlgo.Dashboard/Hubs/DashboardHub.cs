using Microsoft.AspNetCore.SignalR;
using NiftyOptionsAlgo.Core;

namespace NiftyOptionsAlgo.Dashboard.Hubs;

public class DashboardHub : Hub
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardHub> _logger;

    public DashboardHub(IDashboardService dashboardService, ILogger<DashboardHub> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {connectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public async Task GetLivePositions()
    {
        var positions = await _dashboardService.GetOpenPositionsAsync();
        await Clients.Caller.SendAsync("ReceivePositions", positions);
    }

    public async Task GetRiskMetrics()
    {
        var metrics = await _dashboardService.GetRiskMetricsAsync();
        await Clients.All.SendAsync("ReceiveRiskMetrics", metrics);
    }

    public async Task GetTradeHistory(int limit = 20)
    {
        var trades = await _dashboardService.GetRecentTradesAsync(limit);
        await Clients.Caller.SendAsync("ReceiveTradeHistory", trades);
    }

    public async Task SubscribeToUpdates()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "live-updates");
        _logger.LogInformation("Client {connectionId} subscribed to live updates", Context.ConnectionId);
    }

    public async Task UnsubscribeFromUpdates()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "live-updates");
        _logger.LogInformation("Client {connectionId} unsubscribed from live updates", Context.ConnectionId);
    }
}
