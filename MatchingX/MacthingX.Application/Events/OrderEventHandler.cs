using MacthingX.Application.Extensions;
using MatchingX.Core.Entities;
using MatchingX.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedX.Core.Enums;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.DropCopy;

namespace MacthingX.Application.Events;
public sealed class OrderEventHandler :
    INotificationHandler<OrderCanceledEvent>,
    INotificationHandler<OrderTradedEvent>,
    INotificationHandler<OrderOpenedEvent>,
    INotificationHandler<OrderModifiedEvent>
{
    private readonly ILogger<OrderEventHandler> _logger;
    private readonly IBookOfferCache _bookOfferCache;
    private readonly IOrderStopCache _orderStopCache;
    private readonly IPublisherEngine<TradeReport> _publisher;
    public OrderEventHandler(IBookOfferCache orderCache, IOrderStopCache orderStopCache, ILogger<OrderEventHandler> logger, IPublisherEngine<TradeReport> publisher)
    {
        _logger = logger;
        _bookOfferCache = orderCache;
        _orderStopCache = orderStopCache;
        _publisher = publisher;
    }

    public async Task Handle(OrderCanceledEvent @event, CancellationToken cancellationToken)
    {
        await UpdateBookAndStop(@event.Order);
        _publisher.PublishEngine(@event.Order.ToExecutionReport(), cancellationToken);
    }

    public async Task Handle(OrderTradedEvent @event, CancellationToken cancellationToken)
    {
        await UpdateBookAndStop(@event.Order);
        _publisher.PublishEngine(@event.Order.ToExecutionReport(), cancellationToken);
    }

    public async Task Handle(OrderOpenedEvent @event, CancellationToken cancellationToken)
    {
        await UpdateBookAndStop(@event.Order);
        _publisher.PublishEngine(@event.Order.ToExecutionReport(), cancellationToken);
    }

    public async Task Handle(OrderModifiedEvent @event, CancellationToken cancellationToken)
    {
        await UpdateBookAndStop(@event.Order);
        _publisher.PublishEngine(@event.Order.ToExecutionReport(), cancellationToken);
    }

    private async Task UpdateBookAndStop(MatchOrder order)
    {
        await _bookOfferCache.UpsertOrder(order);

        if (order is { OrderType: OrderType.StopLimit | OrderType.Stop })
        {
            await _orderStopCache.UpsertOrderAsync(order);
        }
    }
}