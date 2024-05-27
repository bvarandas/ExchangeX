using MacthingX.Application.Extensions;
using MatchingX.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedX.Core.Interfaces;

namespace MacthingX.Application.Events;
public class ExecutionReportEventHandler : 
    INotificationHandler<OrderOpenedEvent>,
    INotificationHandler<OrderCanceledEvent>
{
    private readonly IDropCopyCache _dropCopyCache;
    private readonly IMarketDataCache _marketDataCache;
    private readonly IBookOfferCache _bookOfferCache;
    private readonly ILogger<ExecutedTradeEventHandler> _logger;
    public ExecutionReportEventHandler(ILogger<ExecutedTradeEventHandler> logger, 
        IDropCopyCache dropCopyCache,
        IMarketDataCache marketDataCache,
        IBookOfferCache bookOfferCache)
    {
        _logger = logger;
        _dropCopyCache = dropCopyCache;
        _marketDataCache = marketDataCache;
        _bookOfferCache = bookOfferCache;
    }
    public Task Handle(OrderOpenedEvent notification, CancellationToken cancellationToken)
    {
        _marketDataCache.AddIncremental(notification.Order.ToMarketData());
        _dropCopyCache.AddExecutionReport(notification.Order.ToExecutionReport());
        _bookOfferCache.AddBookItemAsync(notification.Order.ToBookItem());
        return Task.CompletedTask;
    }
    public Task Handle(OrderCanceledEvent notification, CancellationToken cancellationToken)
    {
        _marketDataCache.AddIncremental(notification.Order.ToMarketData());
        _dropCopyCache.AddExecutionReport(notification.Order.ToExecutionReport());
        _bookOfferCache.RemoveBookItemAsync(notification.Order.ToBookItem());
        return Task.CompletedTask;
    }
}