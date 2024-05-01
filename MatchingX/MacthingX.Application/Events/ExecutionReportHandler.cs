using MacthingX.Application.Extensions;
using MatchingX.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MacthingX.Application.Events;
public class ExecutionReportHandler : 
    INotificationHandler<OrderFilledCommand>,
    INotificationHandler<OrderOpenedEvent>,
    INotificationHandler<OrderRejectedEvent>,
    INotificationHandler<OrderCanceledEvent>
{
    private readonly IDropCopyCache _dropCopyCache;
    private readonly IMarketDataCache _marketDataCache;
    private readonly ILogger<ExecutedTradeEventHandler> _logger;
    public ExecutionReportHandler(ILogger<ExecutedTradeEventHandler> logger, 
        IDropCopyCache dropCopyCache,
        IMarketDataCache marketDataCache)
    {
        _logger = logger;
        _dropCopyCache = dropCopyCache;
        _marketDataCache = marketDataCache;
    }
    public Task Handle(OrderFilledCommand notification, CancellationToken cancellationToken)
    {
        _marketDataCache.AddIncremental(notification.Order.ToMarketData());
        _dropCopyCache.AddExecutionReport(notification.Order.ToExecutionReport());

        return Task.CompletedTask;
    }

    public Task Handle(OrderOpenedEvent notification, CancellationToken cancellationToken)
    {
        _marketDataCache.AddIncremental(notification.Order.ToMarketData());
        _dropCopyCache.AddExecutionReport(notification.Order.ToExecutionReport());
        return Task.CompletedTask;
    }

    public Task Handle(OrderRejectedEvent notification, CancellationToken cancellationToken)
    {
        _marketDataCache.AddIncremental(notification.Order.ToMarketData());
        _dropCopyCache.AddExecutionReport(notification.Order.ToExecutionReport());
        return Task.CompletedTask;
    }

    public Task Handle(OrderCanceledEvent notification, CancellationToken cancellationToken)
    {
        _marketDataCache.AddIncremental(notification.Order.ToMarketData());
        _dropCopyCache.AddExecutionReport(notification.Order.ToExecutionReport());
        return Task.CompletedTask;
    }
}