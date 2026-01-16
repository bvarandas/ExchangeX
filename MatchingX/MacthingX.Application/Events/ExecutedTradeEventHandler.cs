using MacthingX.Application.Extensions;
using MatchingX.Core.Entities;
using MatchingX.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.DropCopy;
using System.Collections.Concurrent;

namespace MacthingX.Application.Events;
public sealed class ExecutedTradeEventHandler :
    INotificationHandler<ExecutedTradeEvent>
{
    private readonly IMatchingCache _cacheMatching;
    private readonly ILogger<ExecutedTradeEventHandler> _logger;
    private readonly IPublisherEngine<TradeReport> _publisher;
    private readonly Thread ThreadExecutedTrade;
    private readonly ConcurrentQueue<MatchingEngine> QueueExecutedOrders;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public ExecutedTradeEventHandler(ILogger<ExecutedTradeEventHandler> logger,
        IMatchingCache cacheMatching,
        IPublisherEngine<TradeReport> publisher
        )
    {
        _logger = logger;
        _cacheMatching = cacheMatching;
        _publisher = publisher;
        _cancellationTokenSource = new CancellationTokenSource();

        QueueExecutedOrders = new ConcurrentQueue<MatchingEngine>();
        ThreadExecutedTrade = new Thread(new ThreadStart(ExecutedTradeOutcome));
        ThreadExecutedTrade.Name = nameof(ExecutedTradeOutcome);
        ThreadExecutedTrade.Start();
    }

    public async Task Handle(ExecutedTradeEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Enviando executed Trade para o Cache Matching!!");

        QueueExecutedOrders.Enqueue(notification.ExecutedOrders);
    }

    private void ExecutedTradeOutcome()
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            if (QueueExecutedOrders.TryDequeue(out MatchingEngine orders))
            {
                var executionReport = orders.ToExecutionReport();

                foreach (var report in executionReport)
                    _publisher.PublishEngine(report.Value, _cancellationTokenSource.Token);
            }


            Thread.Sleep(10);
        }
    }

    ~ExecutedTradeEventHandler()
    {
        _cancellationTokenSource?.Cancel();
    }
}