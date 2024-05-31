﻿using MassTransit;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using SharedX.Core;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.ValueObjects;
using System.Collections.Concurrent;
using System.Threading;

namespace Sharedx.Infra.Outbox.Services;
public class OrderEngineOutboxApp : IConsumer<EnvelopeOutbox<OrderEngine>>
{
    private readonly ILogger<OrderEngineOutboxApp> _logger;
    private readonly PushSocket _sender = null!;
    private readonly ConcurrentQueue<EnvelopeOutbox<OrderEngine>> _queueEnvelopeOutbox = null!;
    private readonly Thread ThreadSenderActivity = null!;
    private readonly CancellationTokenSource _cancellationTokenSource = null!;
    public OrderEngineOutboxApp(ILogger<OrderEngineOutboxApp> logger)
    {
        _logger = logger;
        _queueEnvelopeOutbox = new ConcurrentQueue<EnvelopeOutbox<OrderEngine>>();

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