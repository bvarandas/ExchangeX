using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using QuickFix.Config;
using SharedX.Core.Extensions;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.Specs;
using SharedX.Core.ValueObjects;

namespace Sharedx.Infra.Outbox.Services;
public class OutboxConsumerService<T> : 
    IOutboxConsumerService<T> where T : class,
    IConsumer<T>,
    IHostedService
{
    private readonly IOutboxCache<T> _cacheOutbox;
    private readonly ILogger<OutboxConsumerService<T>> _logger;
    private readonly ConnectionZmq _config;
    private PullSocket _receiver;
    private static Thread ThreadReceiverEnvelope = null!;
    public OutboxConsumerService(
        IOutboxCache<T> cacheOutbox, 
        ILogger<OutboxConsumerService<T>> logger,
        IOptions<ConnectionZmq> options)
    {
        _cacheOutbox = cacheOutbox;
        _logger = logger;
        _config = options.Value;
    }

    protected Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando o Receiver Order ZeroMQ...");

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
                _logger.LogInformation($"Receiver de ordens tentando conectar..{_config.OrderEngineToMatching.Uri}");
                using (_receiver = new PullSocket(">" + _config.OrderEngineToMatching.Uri))
                {
                    _logger.LogInformation("Receiver de ordens Conectado!!!");
                    isConnected = true;

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var message = _receiver.ReceiveMultipartBytes()[0];
                        var envelope = message.DeserializeFromByteArrayProtobuf<EnvelopeOutbox<T>>();

                        var deleted =await _cacheOutbox.DeleteOutboxAsync(envelope);

                        if (deleted.IsSuccess)
                        {
                            /// HACK :Criar interface para receiver geral
                            /// Matching Engine
                            /// Order Engine
                            /// Drop Copy Engine
                            /// Market data Engine
                            /// Trade Engine
                            //_matchReceiver.ReceiveOrder(order);
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
        {
            ///_matchReceiver.ReceiveOrder(order);
        }
    }
}