using MassTransit;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using SharedX.Core;
using SharedX.Core.Matching.MarketData;
using SharedX.Core.ValueObjects;
using System.Collections.Concurrent;
using System.Threading;

namespace Sharedx.Infra.Outbox.Services;
public class MarketDataOutboxApp :  IConsumer<EnvelopeOutbox<MarketData>>
{
    private readonly ILogger<MarketDataOutboxApp> _logger;
    private readonly PushSocket _sender = null!;
    private readonly ConcurrentQueue<EnvelopeOutbox<MarketData>> _queueEnvelopeOutbox = null!;
    private readonly Thread ThreadSenderActivity = null!;
    private readonly CancellationTokenSource _cancellationTokenSource = null!;
    public MarketDataOutboxApp(ILogger<MarketDataOutboxApp> logger)
    {
        _logger = logger;
        _queueEnvelopeOutbox = new ConcurrentQueue<EnvelopeOutbox<MarketData>>();

        _cancellationTokenSource = new CancellationTokenSource();

        ThreadSenderActivity = new Thread(() => SenderActivity(_cancellationTokenSource.Token));
        ThreadSenderActivity.Name = nameof(ThreadSenderActivity);
        ThreadSenderActivity.Start();
    }

    public Task Consume(ConsumeContext<EnvelopeOutbox<MarketData>> context)
    {
        _queueEnvelopeOutbox.Enqueue(context.Message);
        
        return Task.CompletedTask;
    }
    
    public void SenderActivity(CancellationToken cancellationToken )
    {
        while(!cancellationToken.IsCancellationRequested)
        {
            while(_queueEnvelopeOutbox.TryDequeue(out EnvelopeOutbox<MarketData> envelope))
            {
                switch (envelope.ActivityOutbox.NextActivity)
                {
                    case OutboxActivities.OrderEntryToOrderEngineSent:
                        {

                        }
                        break;
                    case OutboxActivities.OrderEngineToMatchingSent:
                        {

                        }
                        break;
                    case OutboxActivities.MatchingToMarketDataSent:
                        {

                        }
                        break;
                    case OutboxActivities.MatchingToOrderEngineSent:
                        {

                        }
                        break;
                    case OutboxActivities.MatchingToDropCopySent:
                        {

                        }
                        break;

                }
            }

            Thread.Sleep(100);
        }
    }
    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
    }
}
