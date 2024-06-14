using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeEngineX.Core.Interfaces;

namespace TradeEngineX.Application.Events;

public class TradeEngineEventHandler :
    INotificationHandler<TradeEngineCreatedEvent>,
    INotificationHandler<TradeEngineUpdatedEvent>,
    INotificationHandler<TradeEngineRemovedEvent>
{

    private readonly ITradeEngineCache _tradeCache = null!;
    private readonly ILogger<TradeEngineEventHandler> _logger;
    public TradeEngineEventHandler(ILogger<TradeEngineEventHandler> logger, ITradeEngineCache tradeCache)
    {
        _tradeCache = tradeCache;
        _logger = logger;
    }
    public async Task Handle(TradeEngineCreatedEvent notification, CancellationToken cancellationToken)
    {
        await _tradeCache.UpsertTradeEngineAsync(notification.TradeReport);
    }

    public async Task Handle(TradeEngineUpdatedEvent notification, CancellationToken cancellationToken)
    {
        await _tradeCache.UpsertTradeEngineAsync(notification.TradeReport);
    }

    public async Task Handle(TradeEngineRemovedEvent notification, CancellationToken cancellationToken)
    {
        await _tradeCache.RemoveTradeEngineAsync(notification.TradeReport);
    }
}
