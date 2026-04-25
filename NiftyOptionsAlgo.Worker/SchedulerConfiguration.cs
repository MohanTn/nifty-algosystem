namespace NiftyOptionsAlgo.Worker;

using NiftyOptionsAlgo.Worker.Jobs;
using Quartz;

public static class SchedulerConfiguration
{
    public static IServiceCollection AddQuartzScheduler(this IServiceCollection services)
    {
        services.AddQuartz(q =>
        {
            // Entry Evaluation Job - Daily at 10:00 AM IST (04:30 UTC)
            var entryJobKey = new JobKey(nameof(EntryEvaluationJob));
            q.AddJob<EntryEvaluationJob>(opts => opts.WithIdentity(entryJobKey));
            q.AddTrigger(opts => opts
                .ForJob(entryJobKey)
                .WithIdentity($"{nameof(EntryEvaluationJob)}-trigger")
                .WithCronSchedule("0 30 4 * * ?", x => x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("UTC"))));

            // Position Monitoring Job - Every 15 seconds during market hours
            var posMonitorJobKey = new JobKey(nameof(PositionMonitoringJob));
            q.AddJob<PositionMonitoringJob>(opts => opts.WithIdentity(posMonitorJobKey));
            q.AddTrigger(opts => opts
                .ForJob(posMonitorJobKey)
                .WithIdentity($"{nameof(PositionMonitoringJob)}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(15).RepeatForever()));

            // Risk Assessment Job - Every minute
            var riskJobKey = new JobKey(nameof(RiskAssessmentJob));
            q.AddJob<RiskAssessmentJob>(opts => opts.WithIdentity(riskJobKey));
            q.AddTrigger(opts => opts
                .ForJob(riskJobKey)
                .WithIdentity($"{nameof(RiskAssessmentJob)}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInMinutes(1).RepeatForever()));

            // Event Calendar Job - Daily at 6:00 AM IST (00:30 UTC)
            var eventJobKey = new JobKey(nameof(EventCalendarJob));
            q.AddJob<EventCalendarJob>(opts => opts.WithIdentity(eventJobKey));
            q.AddTrigger(opts => opts
                .ForJob(eventJobKey)
                .WithIdentity($"{nameof(EventCalendarJob)}-trigger")
                .WithCronSchedule("0 30 0 * * ?", x => x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("UTC"))));

            // VIX Update Job - Every minute
            var vixJobKey = new JobKey(nameof(VixUpdateJob));
            q.AddJob<VixUpdateJob>(opts => opts.WithIdentity(vixJobKey));
            q.AddTrigger(opts => opts
                .ForJob(vixJobKey)
                .WithIdentity($"{nameof(VixUpdateJob)}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInMinutes(1).RepeatForever()));

            // Adjustment Check Job - Every 30 seconds
            var adjustmentJobKey = new JobKey(nameof(AdjustmentCheckJob));
            q.AddJob<AdjustmentCheckJob>(opts => opts.WithIdentity(adjustmentJobKey));
            q.AddTrigger(opts => opts
                .ForJob(adjustmentJobKey)
                .WithIdentity($"{nameof(AdjustmentCheckJob)}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(30).RepeatForever()));

            // Exit Check Job - Every 15 seconds
            var exitJobKey = new JobKey(nameof(ExitCheckJob));
            q.AddJob<ExitCheckJob>(opts => opts.WithIdentity(exitJobKey));
            q.AddTrigger(opts => opts
                .ForJob(exitJobKey)
                .WithIdentity($"{nameof(ExitCheckJob)}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(15).RepeatForever()));

            // Rebalance Job - Weekly on Saturday at 4:00 PM IST (10:30 UTC)
            var rebalanceJobKey = new JobKey(nameof(RebalanceJob));
            q.AddJob<RebalanceJob>(opts => opts.WithIdentity(rebalanceJobKey));
            q.AddTrigger(opts => opts
                .ForJob(rebalanceJobKey)
                .WithIdentity($"{nameof(RebalanceJob)}-trigger")
                .WithCronSchedule("0 30 10 ? * SAT", x => x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("UTC"))));

            // Health Check Job - Every 5 minutes
            var healthJobKey = new JobKey(nameof(HealthCheckJob));
            q.AddJob<HealthCheckJob>(opts => opts.WithIdentity(healthJobKey));
            q.AddTrigger(opts => opts
                .ForJob(healthJobKey)
                .WithIdentity($"{nameof(HealthCheckJob)}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInMinutes(5).RepeatForever()));
        });

        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        return services;
    }
}
