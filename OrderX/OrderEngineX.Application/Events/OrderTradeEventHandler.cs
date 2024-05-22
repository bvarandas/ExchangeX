using MediatR;
using OrderEngineX.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace OrderEngineX.Application.Events;

public class OrderTradeEventHandler :
    IRequestHandler<OrderTradeNewEvent, bool>,
    IRequestHandler<OrderTradeModifyEvent, bool>,
    IRequestHandler<OrderTradeCancelEvent, bool>,
    IRequestHandler<OrderTradeRejectedEvent, bool>
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
    public Task<bool> Handle(OrderTradeNewEvent request, CancellationToken cancellationToken)
    {
        _orderCache.AddOrder(request.Order);
        return Task.FromResult(true);
    }

    public Task<bool> Handle(OrderTradeModifyEvent request, CancellationToken cancellationToken)
    {
        _orderCache.AddOrder(request.Order);
        return Task.FromResult(true);
    }

    public Task<bool> Handle(OrderTradeCancelEvent request, CancellationToken cancellationToken)
    {
        _orderCache.AddOrder(request.Order);
        return Task.FromResult(true);
    }

    public Task<bool> Handle(OrderTradeRejectedEvent request, CancellationToken cancellationToken)
    {
        _reportCache.AddReport(request.Report);
        throw new NotImplementedException();
    }
}
