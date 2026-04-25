namespace NiftyOptionsAlgo.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class EventCalendar : IEventCalendar
{
    private readonly List<MarketEvent> _events = new();

    public EventCalendar()
    {
        InitializeWith2026Events();
    }

    private void InitializeWith2026Events()
    {
        // 2026 RBI MPC dates (6 meetings)
        _events.Add(new MarketEvent { EventDate = new DateTime(2026, 2, 10), Name = "RBI MPC", Category = EventCategory.RbiMpc, RiskLevel = RiskLevel.Extreme });
        _events.Add(new MarketEvent { EventDate = new DateTime(2026, 4, 7), Name = "RBI MPC", Category = EventCategory.RbiMpc, RiskLevel = RiskLevel.Extreme });
        _events.Add(new MarketEvent { EventDate = new DateTime(2026, 6, 9), Name = "RBI MPC", Category = EventCategory.RbiMpc, RiskLevel = RiskLevel.Extreme });
        _events.Add(new MarketEvent { EventDate = new DateTime(2026, 8, 5), Name = "RBI MPC", Category = EventCategory.RbiMpc, RiskLevel = RiskLevel.Extreme });
        _events.Add(new MarketEvent { EventDate = new DateTime(2026, 10, 7), Name = "RBI MPC", Category = EventCategory.RbiMpc, RiskLevel = RiskLevel.Extreme });
        _events.Add(new MarketEvent { EventDate = new DateTime(2026, 12, 8), Name = "RBI MPC", Category = EventCategory.RbiMpc, RiskLevel = RiskLevel.Extreme });

        // 2026 US FOMC dates (6 meetings)
        _events.Add(new MarketEvent { EventDate = new DateTime(2026, 1, 27), Name = "US FOMC", Category = EventCategory.UsaFomc, RiskLevel = RiskLevel.High });
        _events.Add(new MarketEvent { EventDate = new DateTime(2026, 3, 17), Name = "US FOMC", Category = EventCategory.UsaFomc, RiskLevel = RiskLevel.High });
        _events.Add(new MarketEvent { EventDate = new DateTime(2026, 5, 4), Name = "US FOMC", Category = EventCategory.UsaFomc, RiskLevel = RiskLevel.High });
        _events.Add(new MarketEvent { EventDate = new DateTime(2026, 6, 16), Name = "US FOMC", Category = EventCategory.UsaFomc, RiskLevel = RiskLevel.High });
        _events.Add(new MarketEvent { EventDate = new DateTime(2026, 7, 28), Name = "US FOMC", Category = EventCategory.UsaFomc, RiskLevel = RiskLevel.High });
        _events.Add(new MarketEvent { EventDate = new DateTime(2026, 9, 15), Name = "US FOMC", Category = EventCategory.UsaFomc, RiskLevel = RiskLevel.High });
        _events.Add(new MarketEvent { EventDate = new DateTime(2026, 11, 3), Name = "US FOMC", Category = EventCategory.UsaFomc, RiskLevel = RiskLevel.High });
        _events.Add(new MarketEvent { EventDate = new DateTime(2026, 12, 15), Name = "US FOMC", Category = EventCategory.UsaFomc, RiskLevel = RiskLevel.High });

        // Union Budget
        _events.Add(new MarketEvent { EventDate = new DateTime(2026, 2, 1), Name = "Union Budget", Category = EventCategory.IndiaEvent, RiskLevel = RiskLevel.Extreme });

        // Monthly CPI/WPI (first Friday)
        for (int month = 1; month <= 12; month++)
        {
            var firstDay = new DateTime(2026, month, 1);
            int daysToFirstFriday = ((int)DayOfWeek.Friday - (int)firstDay.DayOfWeek + 7) % 7;
            if (daysToFirstFriday == 0) daysToFirstFriday = 7;
            var cpiDate = firstDay.AddDays(daysToFirstFriday);
            _events.Add(new MarketEvent { EventDate = cpiDate, Name = "CPI/WPI", Category = EventCategory.Monthly, RiskLevel = RiskLevel.Medium });
        }

        // Monthly NFP (first Friday after 3rd)
        for (int month = 1; month <= 12; month++)
        {
            var thirdDay = new DateTime(2026, month, 3);
            int daysToNfp = ((int)DayOfWeek.Friday - (int)thirdDay.DayOfWeek + 7) % 7;
            if (daysToNfp == 0) daysToNfp = 7;
            var nfpDate = thirdDay.AddDays(daysToNfp);
            _events.Add(new MarketEvent { EventDate = nfpDate, Name = "US NFP", Category = EventCategory.Monthly, RiskLevel = RiskLevel.Medium });
        }

        // F&O weekly expiry (every Thursday)
        for (int week = 0; week < 52; week++)
        {
            var thursday = new DateTime(2026, 1, 1).AddDays((int)DayOfWeek.Thursday);
            thursday = thursday.AddDays(week * 7);
            if (thursday.Year == 2026)
            {
                _events.Add(new MarketEvent { EventDate = thursday, Name = "F&O Weekly Expiry", Category = EventCategory.Weekly, RiskLevel = RiskLevel.Medium });
            }
        }
    }

    public async Task<List<MarketEvent>> GetUpcomingEventsAsync(int days)
    {
        var today = DateTime.Now;
        return _events.Where(e => e.EventDate >= today && e.EventDate <= today.AddDays(days))
            .OrderBy(e => e.EventDate)
            .ToList();
    }

    public async Task<bool> IsEventWithinWindowAsync(DateTime date, int windowDays)
    {
        return _events.Any(e => e.EventDate >= date && e.EventDate <= date.AddDays(windowDays));
    }

    public async Task<bool> IsSafeToEnterAsync(DateTime entryDate, DateTime expiryDate)
    {
        // Check if any events within 5 days of entry
        var nearbyEvents = _events.Where(e =>
            e.EventDate >= entryDate &&
            e.EventDate <= entryDate.AddDays(5)).ToList();

        return !nearbyEvents.Any();
    }

    public async Task AddCustomEventAsync(MarketEvent marketEvent)
    {
        _events.Add(marketEvent);
    }
}
