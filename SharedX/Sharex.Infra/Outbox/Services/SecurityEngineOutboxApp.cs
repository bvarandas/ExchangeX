using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using SharedX.Core;
using SharedX.Core.Entities;
using SharedX.Core.Extensions;
using SharedX.Core.Specs;
using SharedX.Core.ValueObjects;
using System.Collections.Concurrent;
namespace Sharedx.Infra.Outbox.Services;
public class SecurityEngineOutboxApp : 
    IConsumer<EnvelopeOutbox<SecurityEngine>>

{
    private readonly ILogger<SecurityEngineOutboxApp> _logger;
    private PushSocket _sender = null!;
    private readonly ConcurrentQueue<EnvelopeOutbox<SecurityEngine>> _queueEnvelopeOutbox = null!;
    private readonly Thread ThreadSenderActivity = null!;
    private readonly CancellationTokenSource _cancellationTokenSource = null!;
    private readonly ConnectionZmq _config;
    public SecurityEngineOutboxApp(ILogger<SecurityEngineOutboxApp> logger, IOptions<ConnectionZmq> options)
    {
        _logger = logger;
        _queueEnvelopeOutbox = new ConcurrentQueue<EnvelopeOutbox<SecurityEngine>>();

        _config = options.Value;

        _cancellationTokenSource = new CancellationTokenSource();

        ThreadSenderActivity = new Thread(() => SenderActivity(_cancellationTokenSource.Token));
        ThreadSenderActivity.Name = nameof(ThreadSenderActivity);
        ThreadSenderActivity.Start();
    }

    public Task Consume(ConsumeContext<EnvelopeOutbox<SecurityEngine>> context)
    {
        _queueEnvelopeOutbox.Enqueue(context.Message);
        
        return Task.CompletedTask;
    }
    
    public void SenderActivity(CancellationToken cancellationToken )
    {
        while(!cancellationToken.IsCancellationRequested)
        {
            while(_queueEnvelopeOutbox.TryDequeue(out EnvelopeOutbox<SecurityEngine> envelope))
            {
                switch (envelope.ActivityOutbox.NextActivity)
                {
                    case OutboxActivities.SecurityEngineToMarketDataSent:
                        {
                            using (_sender = new PushSocket("@" + _config.SecurityToMarketData))
                            {
                                var message = envelope.Body.SerializeToByteArrayProtobuf<SecurityEngine>();
                                _sender.SendMultipartBytes(message);
                            }
                        }
                        break;
                    case OutboxActivities.SecurityEngineToMatchingSent:
                        {
                            using (_sender = new PushSocket("@" + _config.SecurityToMatching))
                            {
                                var message = envelope.Body.SerializeToByteArrayProtobuf<SecurityEngine>();
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