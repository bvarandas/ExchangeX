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
public class OutboxConsumerService<T> :
    BackgroundService,
    IOutboxConsumerService<EnvelopeOutbox<T>>,
    IConsumer<EnvelopeOutbox<T>> where T : class

{
    private readonly IOutboxCache<T> _cacheOutbox;
    private readonly IReceiverEngine<T> _receiverEngine;
    private readonly ILogger<OutboxConsumerService<T>> _logger;
    private readonly ConnectionZmq _config;
    private PullSocket _receiver;
    private static Thread ThreadReceiverEnvelope = null!;
    public OutboxConsumerService(
        IOutboxCache<T> cacheOutbox,
        ILogger<OutboxConsumerService<T>> logger,
        IOptions<ConnectionZmq> options,
        IReceiverEngine<T> receiverEngine)
    {
        _cacheOutbox = cacheOutbox;
        _logger = logger;
        _config = options.Value;
        _receiverEngine = receiverEngine;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando o Receiver de Envelopes ZeroMQ...");

        ThreadReceiverEnvelope = new Thread(() => ReceiverEnvelope(cancellationToken));
        ThreadReceiverEnvelope.Name = nameof(ThreadReceiverEnvelope);
        ThreadReceiverEnvelope.Start();

        return Task.CompletedTask;
    }

    private async Task ReceiverEnvelope(CancellationToken stoppingToken)
    {
        bool isConnected = false;
        do
        {
            try
            {
                _logger.LogInformation($"Receiver de envelopes tentando conectar..{_config.ReceiverEngine.Uri}");
                using (_receiver = new PullSocket(">" + _config.ReceiverEngine.Uri))
                {
                    _logger.LogInformation("Receiver de envelopes Conectado!!!");
                    isConnected = true;

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var message = _receiver.ReceiveMultipartBytes()[0];
                        var envelope = message.DeserializeFromByteArrayProtobuf<EnvelopeOutbox<T>>();

                        var deleted = await _cacheOutbox.DeleteOutboxAsync(envelope);

                        if (deleted.IsSuccess)
                        {
                            _receiverEngine.ReceiveEngine(envelope.Body, stoppingToken);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                isConnected = false;
                _logger.LogError(ex.Message, ex);
            }

            Thread.Sleep(100);
        } while (!isConnected);

    }

    public async Task Consume(ConsumeContext<EnvelopeOutbox<T>> context)
    {
        var envelope = context.Message;

        var deleted = await _cacheOutbox.DeleteOutboxAsync(envelope);

        if (deleted.IsSuccess)
            _receiverEngine.ReceiveEngine(envelope.Body, default(CancellationToken));
    }


}