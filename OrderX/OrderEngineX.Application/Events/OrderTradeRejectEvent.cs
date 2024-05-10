using SharedX.Core.Entities;
using SharedX.Core.Events;
using SharedX.Core.Matching.OrderEngine;
namespace OrderEngineX.Application.Events;
public class OrderTradeRejectEvent : Event
{
    public readonly ReportFix Report;
    public DateTime Timestamp { get; private set; }
    public OrderTradeRejectEvent(ReportFix report)
    {
        Report = report;
        Timestamp = DateTime.Now;
    }
}
