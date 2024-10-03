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
    BackgroundService,
    IOutboxPublisherService<T> where T : class

{
    private readonly IOutboxCache<T> _cacheOutbox;
    private readonly ILogger<OutboxPublisherService<T>> _logger;
    private readonly IBus _bus;
    private PushSocket _sender;
    private readonly ConnectionZmq _configZmq;
    private readonly ConnectionRmq _configRmq;
    private static Thread ThreadSenderRabbitMQEnvelope = null!;
    private static Thread ThreadSenderZeroMQEnvelope = null!;
    private static ISendEndpoint _sendEndpoint;
    public OutboxPublisherService(
        ILogger<OutboxPublisherService<T>> logger,
        IOutboxCache<T> cacheOutbox,
        IOptions<ConnectionZmq> optionsZmq,
        IOptions<ConnectionRmq> optionsRmq,
        IBus bus)
    {
        _configZmq = optionsZmq.Value;
        _configRmq = optionsRmq.Value;
        _cacheOutbox = cacheOutbox;
        _logger = logger;
        _bus = bus;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
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
        _logger.LogInformation($"Iniciando o Publisher RabbitMQ para o engine de {typeof(T)} ...");

        _sendEndpoint = await _bus.GetSendEndpoint(new Uri(_configRmq.PublisherEngine.Uri));

        while (!cancellationToken.IsCancellationRequested)
        {
            while (_cacheOutbox.TryDequeueRabbitMQEnvelope(out EnvelopeOutbox<T> envelope).Result.IsSuccess)
            {
                var inserted = await _cacheOutbox.UpsertOutboxAsync(envelope);

                if (inserted.IsSuccess)
                    await _sendEndpoint.Send(envelope);

            }

            Thread.Sleep(10);
        }
    }

    private async void SenderZeroMQEnvelope(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando o Publisher ZeroMQ...");

        using (_sender = new PushSocket("@" + _configZmq.PublisherEngine.Uri))
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


            }
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}
