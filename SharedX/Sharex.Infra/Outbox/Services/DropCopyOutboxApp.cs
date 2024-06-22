using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using SharedX.Core;
using SharedX.Core.Extensions;
using SharedX.Core.Matching.DropCopy;
using SharedX.Core.Specs;
using SharedX.Core.ValueObjects;
using System.Collections.Concurrent;
namespace Sharedx.Infra.Outbox.Services;
public class DropCopyOutboxApp : IConsumer<EnvelopeOutbox<ExecutionReport>>
{
    private readonly ILogger<DropCopyOutboxApp> _logger;
    private PushSocket _sender = null!;
    private readonly ConcurrentQueue<EnvelopeOutbox<ExecutionReport>> _queueEnvelopeOutbox = null!;
    private readonly Thread ThreadSenderActivity = null!;
    private readonly CancellationTokenSource _cancellationTokenSource = null!;
    private readonly ConnectionZmq _config;
    public DropCopyOutboxApp(ILogger<DropCopyOutboxApp> logger, IOptions<ConnectionZmq> options)
    {
        _logger = logger;
        _queueEnvelopeOutbox = new ConcurrentQueue<EnvelopeOutbox<ExecutionReport>>();

        _config = options.Value;

        _cancellationTokenSource = new CancellationTokenSource();

        ThreadSenderActivity = new Thread(() => SenderActivity(_cancellationTokenSource.Token));
        ThreadSenderActivity.Name = nameof(ThreadSenderActivity);
        ThreadSenderActivity.Start();
    }

    public Task Consume(ConsumeContext<EnvelopeOutbox<ExecutionReport>> context)
    {
        _queueEnvelopeOutbox.Enqueue(context.Message);
        
        return Task.CompletedTask;
    }
    
    public void SenderActivity(CancellationToken cancellationToken )
    {
        while(!cancellationToken.IsCancellationRequested)
        {
            while(_queueEnvelopeOutbox.TryDequeue(out EnvelopeOutbox<ExecutionReport> envelope))
            {
                switch (envelope.ActivityOutbox.Activity)
                {
                    case OutboxActivities.MatchingToDropCopySent:
                        {
                            using (_sender = new PushSocket("@" + _config.MatchingToDropCopy))
                            {
                                var message = envelope.Body.SerializeToByteArrayProtobuf<ExecutionReport>();
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