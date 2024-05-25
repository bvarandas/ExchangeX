using MediatR;
using OrderEngineX.Core.Interfaces;
using Microsoft.Extensions.Logging;
namespace OrderEngineX.Application.Events;
public class OrderTradeEventHandler :
    INotificationHandler<OrderTradeNewEvent>,
    INotificationHandler<OrderTradeModifyEvent>,
    INotificationHandler<OrderTradeCancelEvent>,
    INotificationHandler<OrderTradeRejectedEvent>
{
    private readonly IOrderReportCache _reportCache;
    private readonly IOrderEngineCache _orderCache;
    private readonly ILogger<OrderTradeEventHandler> _logger;
    public OrderTradeEventHandler(IOrderEngineCache orderCache, 
        IOrderReportCache reportCache, 
        ILogger<OrderTradeEventHandler> logger)
    {
        _logger = logger;
        _orderCache = orderCache;
        _reportCache = reportCache;
    }
    public Task Handle(OrderTradeNewEvent request, CancellationToken cancellationToken)
    {
        _orderCache.AddOrder(request.Order);
        return Task.CompletedTask;
    }
    public Task Handle(OrderTradeModifyEvent request, CancellationToken cancellationToken)
    {
        _orderCache.AddOrder(request.Order);
        return Task.CompletedTask;
    }
    public Task Handle(OrderTradeCancelEvent request, CancellationToken cancellationToken)
    {
        _orderCache.AddOrder(request.Order);
        return Task.CompletedTask;
    }
    public Task Handle(OrderTradeRejectedEvent request, CancellationToken cancellationToken)
    {
        _reportCache.AddReport(request.Report);
        return Task.CompletedTask;
    }
}
