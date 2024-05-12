using SharedX.Core.Entities;
using SharedX.Core.Events;

namespace MacthingX.Application.Events;

public class OrderRejectEvent : Event
{
    public readonly ReportFix Report;
    public DateTime Timestamp { get; private set; }
    public OrderRejectEvent(ReportFix report)
    {
        Report = report;
        Timestamp = DateTime.Now;
    }
}
