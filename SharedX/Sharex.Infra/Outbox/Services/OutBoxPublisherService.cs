using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using SharedX.Core.Extensions;
using SharedX.Core.Interfaces;
using SharedX.Core.Specs;
using SharedX.Core.ValueObjects;

namespace Sharedx.Infra.Outbox.Services;
public class OutBoxPublisherService<T> : 
    IOutboxPublisherService<T> where T : class,
    IHostedService
{
    private readonly IOutboxCache<T> _cacheOutbox;
    private readonly ILogger<OutBoxPublisherService<T>> _logger;
    private readonly IBus _bus;
    private PushSocket _sender;
    private readonly ConnectionZmq _config;
    private static Thread ThreadSenderEnvelope = null!;
    private static Thread ThreadSenderZeroMQEnvelope = null!;
    public OutBoxPublisherService(
        ILogger<OutBoxPublisherService<T>> logger,
        IOutboxCache<T> cacheOutbox,
        IOptions<ConnectionZmq> options,
        IBus bus)
    {
        _config = options.Value;
        _cacheOutbox = cacheOutbox;
        _logger = logger;
        _bus = bus;
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando o Sender Report ZeroMQ...");

        ThreadSenderEnvelope = new Thread(() => SenderEnvelope(cancellationToken));
        ThreadSenderEnvelope.Name = nameof(ThreadSenderEnvelope);
        ThreadSenderEnvelope.Start();

        ThreadSenderZeroMQEnvelope = new Thread(() => SenderZeroMQEnvelope(cancellationToken));
        ThreadSenderZeroMQEnvelope.Name = nameof(ThreadSenderZeroMQEnvelope);
        ThreadSenderZeroMQEnvelope.Start();

        return Task.CompletedTask;
    }

    private void SenderZeroMQEnvelope(CancellationToken cancellationToken)
    {
        while (_cacheOutbox.TryDequeueRabbitMQEnvelope(out EnvelopeOutbox<T> report).Result.IsSuccess)
        {
            var message = report.SerializeToByteArrayProtobuf<EnvelopeOutbox<T>>();
            _bus.Publish(message);
        }
    }

    private void SenderEnvelope(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando o Publisher ZeroMQ...");

        using (_sender = new PushSocket(_config.MatchingToDropCopy.Uri))
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                while (_cacheOutbox.TryDequeueZeroMQEnvelope(out EnvelopeOutbox<T> envelope).Result.IsSuccess)
                {
                    var message = envelope.SerializeToByteArrayProtobuf<EnvelopeOutbox<T>>();
                    _sender.SendMultipartBytes(message);
                }

                Thread.Sleep(10);
            }
        }
    }
    protected Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}
