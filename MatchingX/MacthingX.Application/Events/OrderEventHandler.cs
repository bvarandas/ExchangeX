using Amazon.Runtime.Internal.Util;
using MacthingX.Application.Services;
using MatchingX.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedX.Core.Enums;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;

namespace MacthingX.Application.Events;
public class OrderEventHandler :    
    INotificationHandler<OrderCanceledEvent>,
    INotificationHandler<OrderTradedEvent>,
    INotificationHandler<OrderOpenedEvent>,
    INotificationHandler<OrderRejectedEvent>,
    INotificationHandler<OrderModifiedEvent>
{
    private readonly ILogger<OrderEventHandler> _logger;
    private readonly IMatchingCache _matchCache;
    private readonly IOrderStopCache _orderStopCache;
    
    public OrderEventHandler(IMatchingCache orderCache, IOrderStopCache orderStopCache, ILogger<OrderEventHandler> logger) 
    {
        _logger = logger;
        _matchCache = orderCache;
        _orderStopCache = orderStopCache;
    }

    
    public async Task Handle(OrderCanceledEvent @event, CancellationToken cancellationToken)
    {
        await _matchCache.DeleteSellOrderAsync(@event.Order.Symbol, @event.Order.OrderID);

        if (@event.Order.OrderType == OrderType.StopLimit || 
            @event.Order.OrderType == OrderType.Stop)
        {
            await _orderStopCache.DeleteOrderAsync(@event.Order.Symbol, @event.Order.OrderID);
        }
    }

    public async Task Handle(OrderTradedEvent @event, CancellationToken cancellationToken)
    {
        await _matchCache.DeleteSellOrderAsync(@event.Order.Symbol, @event.Order.OrderID);

        if (@event.Order.OrderType == OrderType.StopLimit ||
            @event.Order.OrderType == OrderType.Stop)
        {
            await _orderStopCache.DeleteOrderAsync(@event.Order.Symbol, @event.Order.OrderID);
        }
    }

    public Task Handle(OrderOpenedEvent @event, CancellationToken cancellationToken)
    {
        if (@event.Order.Side == SideTrade.Buy)
            _matchCache.UpsertBuyOrder(@event.Order);
        else if (@event.Order.Side == SideTrade.Sell)
            _matchCache.UpsertSellOrder(@event.Order);


        return Task.CompletedTask;
    }

    public async Task Handle(OrderRejectedEvent @event, CancellationToken cancellationToken)
    {
        await _matchCache.DeleteSellOrderAsync(@event.Order.Symbol, @event.Order.OrderID);

        if (@event.Order.OrderType == OrderType.StopLimit ||
            @event.Order.OrderType == OrderType.Stop)
        {
            await _orderStopCache.DeleteOrderAsync(@event.Order.Symbol, @event.Order.OrderID);
        }
    }

    public async Task Handle(OrderModifiedEvent @event, CancellationToken cancellationToken)
    {
        if (@event.Order.Side == SideTrade.Buy)
            _matchCache.UpsertBuyOrder(@event.Order);
        else if (@event.Order.Side == SideTrade.Sell)
            _matchCache.UpsertSellOrder(@event.Order);

        if (@event.Order.OrderType == OrderType.StopLimit ||
            @event.Order.OrderType == OrderType.Stop)
        {
            _orderStopCache.UpsertOrderAsync(@event.Order);
        }

    }
}