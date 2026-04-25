namespace NiftyOptionsAlgo.Worker.Jobs;

using NiftyOptionsAlgo.Core;
using Quartz;

public class EventCalendarJob : IJob
{
    private readonly IEventCalendar _eventCalendar;
    private readonly ILogger<EventCalendarJob> _logger;

    public EventCalendarJob(IEventCalendar eventCalendar, ILogger<EventCalendarJob> logger)
    {
        _eventCalendar = eventCalendar;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            _logger.LogDebug("EventCalendarJob executing at {time}", DateTime.UtcNow);
            var upcomingEvents = await _eventCalendar.GetUpcomingEventsAsync(7);

            if (upcomingEvents.Count > 0)
            {
                _logger.LogInformation("Upcoming events in 7 days: {count}", upcomingEvents.Count);
                foreach (var evt in upcomingEvents)
                {
                    _logger.LogInformation("Event: {name} on {date:yyyy-MM-dd}",
                        evt.Name, evt.EventDate);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in EventCalendarJob");
        }
    }
}
