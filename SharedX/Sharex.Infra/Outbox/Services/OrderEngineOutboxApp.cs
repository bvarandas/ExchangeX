using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using SharedX.Core;
using SharedX.Core.Extensions;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.Specs;
using SharedX.Core.ValueObjects;
using System.Collections.Concurrent;

namespace Sharedx.Infra.Outbox.Services;
public class OrderEngineOutboxApp : IConsumer<EnvelopeOutbox<OrderEngine>>
{
    private readonly ILogger<OrderEngineOutboxApp> _logger;
    private PushSocket _sender = null!;
    private readonly ConcurrentQueue<EnvelopeOutbox<OrderEngine>> _queueEnvelopeOutbox = null!;
    private readonly Thread ThreadSenderActivity = null!;
    private readonly CancellationTokenSource _cancellationTokenSource = null!;
    private readonly ConnectionZmq _config;
    public OrderEngineOutboxApp(ILogger<OrderEngineOutboxApp> logger, IOptions<ConnectionZmq> options)
    {
        _logger = logger;
        _queueEnvelopeOutbox = new ConcurrentQueue<EnvelopeOutbox<OrderEngine>>();

        _config = options.Value;

        _cancellationTokenSource = new CancellationTokenSource();

        ThreadSenderActivity = new Thread(() => SenderActivity(_cancellationTokenSource.Token));
        ThreadSenderActivity.Name = nameof(ThreadSenderActivity);
        ThreadSenderActivity.Start();
    }

    public Task Consume(ConsumeContext<EnvelopeOutbox<OrderEngine>> context)
    {
        _queueEnvelopeOutbox.Enqueue(context.Message);
        
        return Task.CompletedTask;
    }
    
    public void SenderActivity(CancellationToken cancellationToken )
    {
        while(!cancellationToken.IsCancellationRequested)
        {
            while(_queueEnvelopeOutbox.TryDequeue(out EnvelopeOutbox<OrderEngine> envelope))
            {
                switch (envelope.ActivityOutbox.NextActivity)
                {
                    case OutboxActivities.OrderEntryToOrderEngineSent:
                        {
                            using (_sender = new PushSocket("@" + _config.OrderEntryToOrderEngine))
                            {
                                var message = envelope.Body.SerializeToByteArrayProtobuf<OrderEngine>();
                                _sender.SendMultipartBytes(message);
                            }
                        }
                        break;
                    case OutboxActivities.OrderEngineToMatchingSent:
                        {
                            using (_sender = new PushSocket("@" + _config.OrderEngineToMatching))
                            {
                                var message = envelope.Body.SerializeToByteArrayProtobuf<OrderEngine>();
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