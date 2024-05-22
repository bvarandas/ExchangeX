using SharedX.Core.Entities;
using SharedX.Core.Events;
namespace OrderEngineX.Application.Events;
public class OrderTradeRejectedEvent : Event
{
    public readonly ReportFix Report;
    public DateTime Timestamp { get; private set; }
    public OrderTradeRejectedEvent(ReportFix report)
    {
        Report = report;
        Timestamp = DateTime.Now;
    }
}