using MatchingX.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedX.Core.Enums;

namespace MacthingX.Application.Events;
public class OrderEventHandler :
    INotificationHandler<OrderCanceledEvent>,
    INotificationHandler<OrderTradedEvent>,
    INotificationHandler<OrderOpenedEvent>,
    INotificationHandler<OrderModifiedEvent>
{
    private readonly ILogger<OrderEventHandler> _logger;
    private readonly IBookOfferCache _bookOfferCache;
    private readonly IOrderStopCache _orderStopCache;

    public OrderEventHandler(IBookOfferCache orderCache, IOrderStopCache orderStopCache, ILogger<OrderEventHandler> logger)
    {
        _logger = logger;
        _bookOfferCache = orderCache;
        _orderStopCache = orderStopCache;
    }


    public async Task Handle(OrderCanceledEvent @event, CancellationToken cancellationToken)
    {
        if (@event is { Order.Side: SideTrade.Sell })
            await _bookOfferCache.DeleteSellOrderAsync(@event.Order.Symbol, @event.Order.OrderID);
        else
            await _bookOfferCache.DeleteBuyOrderAsync(@event.Order.Symbol, @event.Order.OrderID);

        if (@event is { Order.OrderType: OrderType.StopLimit | OrderType.Stop })
        {
            if (@event is { Order.Side: SideTrade.Sell })
                await _orderStopCache.DeleteSellOrderAsync(@event.Order.Symbol, @event.Order.OrderID);
            else
                await _orderStopCache.DeleteBuyOrderAsync(@event.Order.Symbol, @event.Order.OrderID);
        }
    }

    public async Task Handle(OrderTradedEvent @event, CancellationToken cancellationToken)
    {
        if (@event is { Order.Side: SideTrade.Sell })
            await _bookOfferCache.DeleteSellOrderAsync(@event.Order.Symbol, @event.Order.OrderID);
        else
            await _bookOfferCache.DeleteBuyOrderAsync(@event.Order.Symbol, @event.Order.OrderID);

        if (@event is { Order.OrderType: OrderType.StopLimit | OrderType.Stop })
        {
            if (@event is { Order.Side: SideTrade.Sell })
                await _orderStopCache.DeleteSellOrderAsync(@event.Order.Symbol, @event.Order.OrderID);
            else
                await _orderStopCache.DeleteBuyOrderAsync(@event.Order.Symbol, @event.Order.OrderID);
        }
    }

    public async Task Handle(OrderOpenedEvent @event, CancellationToken cancellationToken)
    {
        if (@event.Order.Side == SideTrade.Buy)
        {
            await _bookOfferCache.UpsertBuyOrder(@event.Order);

            if (@event is { Order.OrderType: OrderType.Stop | OrderType.StopLimit })
                await _orderStopCache.UpsertBuyOrderAsync(@event.Order);
        }
        else if (@event.Order.Side == SideTrade.Sell)
        {
            await _bookOfferCache.UpsertSellOrder(@event.Order);

            if (@event is { Order.OrderType: OrderType.Stop | OrderType.StopLimit })
                await _orderStopCache.UpsertBuyOrderAsync(@event.Order);
        }
    }

    public async Task Handle(OrderModifiedEvent @event, CancellationToken cancellationToken)
    {
        if (@event is { Order.Side: SideTrade.Buy })
            await _bookOfferCache.UpsertBuyOrder(@event.Order);
        else
            await _bookOfferCache.UpsertSellOrder(@event.Order);

        if (@event is { Order.OrderType: OrderType.StopLimit | OrderType.Stop })
        {
            if (@event is { Order.Side: SideTrade.Sell })
                await _orderStopCache.UpsertSellOrderAsync(@event.Order);
            else
                await _orderStopCache.UpsertBuyOrderAsync(@event.Order);
        }

    }
}