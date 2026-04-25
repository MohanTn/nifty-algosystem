using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using NiftyOptionsAlgo.Core;
using NiftyOptionsAlgo.Engine;
using NiftyOptionsAlgo.Infrastructure;
using NiftyOptionsAlgo.RiskManager;
using NiftyOptionsAlgo.Dashboard.Hubs;
using NiftyOptionsAlgo.Dashboard.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSignalR();

// Register core services
builder.Services.AddScoped<IGreeksCalculator, BlackScholesGreeksCalculator>();
builder.Services.AddScoped<IStrategyEngine, StrategyEngine>();
builder.Services.AddScoped<IPositionMonitor, PositionMonitor>();
builder.Services.AddScoped<IOrderExecutor, OrderExecutor>();
builder.Services.AddScoped<IRiskManager, RiskManager>();
builder.Services.AddScoped<IEventCalendar, EventCalendar>();
builder.Services.AddScoped<IKiteService, KiteService>();
builder.Services.AddScoped<ITickerService, TickerService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Register dashboard services
builder.Services.AddScoped<IDashboardService, DashboardService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapHub<DashboardHub>("/dashboardhub");
app.MapFallbackToPage("/_Host");

app.Run();
