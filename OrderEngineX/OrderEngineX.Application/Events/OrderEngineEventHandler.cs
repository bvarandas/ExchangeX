using MediatR;
using Microsoft.Extensions.Logging;
using OrderEngineX.Core.Interfaces;
namespace OrderEngineX.Application.Events;
public class OrderEngineEventHandler :
    INotificationHandler<OrderEngineNewEvent>,
    INotificationHandler<OrderTradeModifyEvent>,
    INotificationHandler<OrderEngineCancelEvent>,
    INotificationHandler<OrderEngineRejectedEvent>
{
    private readonly IOrderReportCache _reportCache;
    private readonly IOrderEngineCache _orderCache;
    private readonly ILogger<OrderEngineEventHandler> _logger;
    public OrderEngineEventHandler(IOrderEngineCache orderCache,
        IOrderReportCache reportCache,
        ILogger<OrderEngineEventHandler> logger)
    {
        _logger = logger;
        _orderCache = orderCache;
        _reportCache = reportCache;
    }
    public Task Handle(OrderEngineNewEvent request, CancellationToken cancellationToken)
    {
        _orderCache.AddOrder(request.Order);
        return Task.CompletedTask;
    }
    public Task Handle(OrderTradeModifyEvent request, CancellationToken cancellationToken)
    {
        _orderCache.AddOrder(request.Order);
        return Task.CompletedTask;
    }
    public Task Handle(OrderEngineCancelEvent request, CancellationToken cancellationToken)
    {
        _orderCache.AddOrder(request.Order);
        return Task.CompletedTask;
    }
    public Task Handle(OrderEngineRejectedEvent request, CancellationToken cancellationToken)
    {
        _reportCache.AddReport(request.Report);
        return Task.CompletedTask;
    }
}
