using SharedX.Core.Entities;
using SharedX.Core.Events;
namespace OrderEngineX.Application.Events;
public class OrderEngineRejectedEvent : Event
{
    public readonly ReportFix Report;
    public DateTime Timestamp { get; private set; }
    public OrderEngineRejectedEvent(ReportFix report)
    {
        Report = report;
        Timestamp = DateTime.Now;
    }
}