using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using SharedX.Core;
using SharedX.Core.Entities;
using SharedX.Core.Extensions;
using SharedX.Core.Matching.MarketData;
using SharedX.Core.Specs;
using SharedX.Core.ValueObjects;
using System.Collections.Concurrent;
using System.Threading;

namespace Sharedx.Infra.Outbox.Services;
public class MarketDataOutboxApp :  IConsumer<EnvelopeOutbox<MarketData>>
{
    private readonly ILogger<MarketDataOutboxApp> _logger;
    private PushSocket _sender = null!;
    private readonly ConcurrentQueue<EnvelopeOutbox<MarketData>> _queueEnvelopeOutbox = null!;
    private readonly Thread ThreadSenderActivity = null!;
    private readonly CancellationTokenSource _cancellationTokenSource = null!;
    private readonly ConnectionZmq _config;
    public MarketDataOutboxApp(ILogger<MarketDataOutboxApp> logger, IOptions<ConnectionZmq> options)
    {
        _logger = logger;
        _queueEnvelopeOutbox = new ConcurrentQueue<EnvelopeOutbox<MarketData>>();

        _config = options.Value;

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
                    case OutboxActivities.MatchingToMarketDataSent:
                        {
                            using (_sender = new PushSocket("@" + _config.MatchingToMarketData))
                            {
                                var message = envelope.Body.SerializeToByteArrayProtobuf<MarketData>();
                                _sender.SendMultipartBytes(message);
                            }
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
