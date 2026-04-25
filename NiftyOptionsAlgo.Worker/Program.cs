using NiftyOptionsAlgo.Core;
using NiftyOptionsAlgo.Engine;
using NiftyOptionsAlgo.Infrastructure;
using NiftyOptionsAlgo.RiskManager;
using NiftyOptionsAlgo.Worker;
using Serilog;
using Serilog.Events;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Services.AddSerilog();

// Register configuration
builder.Services.AddSingleton<StrategyConfig>();

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

// Configure Quartz scheduler
builder.Services.AddQuartzScheduler();

var host = builder.Build();
host.Run();
