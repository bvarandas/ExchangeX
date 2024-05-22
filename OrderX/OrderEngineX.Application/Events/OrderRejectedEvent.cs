using SharedX.Core.Entities;
using SharedX.Core.Events;
namespace OrderEngineX.Application.Events;
public class OrderRejectedEvent : Event
{
    public readonly ReportFix Report;
    public DateTime Timestamp { get; private set; }
    public OrderRejectedEvent(ReportFix report)
    {
        Report = report;
        Timestamp = DateTime.Now;
    }
}