using MassTransit;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using SharedX.Core;
using SharedX.Core.Matching.DropCopy;
using SharedX.Core.ValueObjects;
using System.Collections.Concurrent;
using System.Threading;

namespace Sharedx.Infra.Outbox.Services;
public class DropCopyOutboxApp : IConsumer<EnvelopeOutbox<ExecutionReport>>
{
    private readonly ILogger<DropCopyOutboxApp> _logger;
    private readonly PushSocket _sender = null!;
    private readonly ConcurrentQueue<EnvelopeOutbox<ExecutionReport>> _queueEnvelopeOutbox = null!;
    private readonly Thread ThreadSenderActivity = null!;
    private readonly CancellationTokenSource _cancellationTokenSource = null!;
    public DropCopyOutboxApp(ILogger<DropCopyOutboxApp> logger)
    {
        _logger = logger;
        _queueEnvelopeOutbox = new ConcurrentQueue<EnvelopeOutbox<ExecutionReport>>();

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