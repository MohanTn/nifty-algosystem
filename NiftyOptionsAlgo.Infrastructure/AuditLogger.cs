namespace NiftyOptionsAlgo.Infrastructure;

using Microsoft.Extensions.Logging;
using NiftyOptionsAlgo.Core;

public interface IAuditLogger
{
    Task LogTradeEntryAsync(StrangleTrade trade, string reason);
    Task LogTradeExitAsync(Guid tradeId, ExitReason reason, decimal pnl);
    Task LogAdjustmentAsync(Guid tradeId, int oldStrike, int newStrike);
    Task LogEmergencyExitAsync(string reason);
    Task LogRiskBreachAsync(string riskMetric, decimal value, decimal threshold);
}

public class AuditLogger : IAuditLogger
{
    private readonly ILogger<AuditLogger> _logger;
    private readonly string _auditLogPath = "logs/audit.log";

    public AuditLogger(ILogger<AuditLogger> logger)
    {
        _logger = logger;
    }

    public async Task LogTradeEntryAsync(StrangleTrade trade, string reason)
    {
        var entry = new AuditEntry
        {
            Timestamp = DateTime.UtcNow,
            EventType = "TRADE_ENTRY",
            TradeId = trade.Id.ToString(),
            Details = $"PE Strike: {trade.Legs.FirstOrDefault(l => l.Type == LegType.ShortPE)?.Strike}, " +
                     $"CE Strike: {trade.Legs.FirstOrDefault(l => l.Type == LegType.ShortCE)?.Strike}, " +
                     $"Premium: {trade.TotalPremiumCollected}, Reason: {reason}",
            Status = "SUCCESS"
        };

        await WriteAuditLog(entry);
        _logger.LogInformation("AUDIT: Trade entry {tradeId} - {reason}", trade.Id, reason);
    }

    public async Task LogTradeExitAsync(Guid tradeId, ExitReason reason, decimal pnl)
    {
        var entry = new AuditEntry
        {
            Timestamp = DateTime.UtcNow,
            EventType = "TRADE_EXIT",
            TradeId = tradeId.ToString(),
            Details = $"Exit Reason: {reason}, P&L: {pnl}",
            Status = "SUCCESS"
        };

        await WriteAuditLog(entry);
        _logger.LogInformation("AUDIT: Trade exit {tradeId} - {reason} P&L: {pnl}", tradeId, reason, pnl);
    }

    public async Task LogAdjustmentAsync(Guid tradeId, int oldStrike, int newStrike)
    {
        var entry = new AuditEntry
        {
            Timestamp = DateTime.UtcNow,
            EventType = "ADJUSTMENT",
            TradeId = tradeId.ToString(),
            Details = $"Old Strike: {oldStrike}, New Strike: {newStrike}",
            Status = "SUCCESS"
        };

        await WriteAuditLog(entry);
        _logger.LogInformation("AUDIT: Adjustment {tradeId} - {oldStrike} → {newStrike}",
            tradeId, oldStrike, newStrike);
    }

    public async Task LogEmergencyExitAsync(string reason)
    {
        var entry = new AuditEntry
        {
            Timestamp = DateTime.UtcNow,
            EventType = "EMERGENCY_EXIT",
            Details = $"Reason: {reason}",
            Status = "CRITICAL"
        };

        await WriteAuditLog(entry);
        _logger.LogCritical("AUDIT: Emergency exit triggered - {reason}", reason);
    }

    public async Task LogRiskBreachAsync(string riskMetric, decimal value, decimal threshold)
    {
        var entry = new AuditEntry
        {
            Timestamp = DateTime.UtcNow,
            EventType = "RISK_BREACH",
            Details = $"Metric: {riskMetric}, Value: {value}, Threshold: {threshold}",
            Status = "WARNING"
        };

        await WriteAuditLog(entry);
        _logger.LogWarning("AUDIT: Risk breach - {metric} {value} > {threshold}",
            riskMetric, value, threshold);
    }

    private async Task WriteAuditLog(AuditEntry entry)
    {
        try
        {
            var logMessage = $"{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff} | {entry.EventType} | " +
                           $"{entry.Status} | TradeId: {entry.TradeId} | {entry.Details}";

            var directory = Path.GetDirectoryName(_auditLogPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.AppendAllTextAsync(_auditLogPath, logMessage + Environment.NewLine);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write audit log");
        }
    }
}

public class AuditEntry
{
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string TradeId { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
