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
public class OutboxPublisherService<T> :
    IOutboxPublisherService<T> where T : class,
    IHostedService
{
    private readonly IOutboxCache<T> _cacheOutbox;
    private readonly ILogger<OutboxPublisherService<T>> _logger;
    private readonly IBus _bus;
    private PushSocket _sender;
    private readonly ConnectionZeroMq _config;
    private static Thread ThreadSenderRabbitMQEnvelope = null!;
    private static Thread ThreadSenderZeroMQEnvelope = null!;
    public OutboxPublisherService(
        ILogger<OutboxPublisherService<T>> logger,
        IOutboxCache<T> cacheOutbox,
        IOptions<ConnectionZeroMq> options,
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

        ThreadSenderRabbitMQEnvelope = new Thread(() => SenderRabbitMqEnvelope(cancellationToken));
        ThreadSenderRabbitMQEnvelope.Name = nameof(ThreadSenderRabbitMQEnvelope);
        ThreadSenderRabbitMQEnvelope.Start();

        ThreadSenderZeroMQEnvelope = new Thread(() => SenderZeroMQEnvelope(cancellationToken));
        ThreadSenderZeroMQEnvelope.Name = nameof(ThreadSenderZeroMQEnvelope);
        ThreadSenderZeroMQEnvelope.Start();

        return Task.CompletedTask;
    }

    private async void SenderRabbitMqEnvelope(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando o Publisher RabbitMQ...");

        while (!cancellationToken.IsCancellationRequested)
        {
            while (_cacheOutbox.TryDequeueRabbitMQEnvelope(out EnvelopeOutbox<T> envelope).Result.IsSuccess)
            {
                var inserted = await _cacheOutbox.UpsertOutboxAsync(envelope);
                if (inserted.IsSuccess)
                {
                    var message = envelope.SerializeToByteArrayProtobuf<EnvelopeOutbox<T>>();
                    await _bus.Publish(message);
                }
            }

            Thread.Sleep(10);
        }
    }

    private async void SenderZeroMQEnvelope(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando o Publisher ZeroMQ...");

        using (_sender = new PushSocket(_config.PushPullAddress.Uri))
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                while (_cacheOutbox.TryDequeueZeroMQEnvelope(out EnvelopeOutbox<T> envelope).Result.IsSuccess)
                {
                    var inserted = await _cacheOutbox.UpsertOutboxAsync(envelope);
                    if (inserted.IsSuccess)
                    {
                        var message = envelope.SerializeToByteArrayProtobuf<EnvelopeOutbox<T>>();
                        _sender.SendMultipartBytes(message);
                    }
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
