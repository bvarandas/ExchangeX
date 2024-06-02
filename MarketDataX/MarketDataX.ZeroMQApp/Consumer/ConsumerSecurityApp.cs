using MarketDataX.Core.Interfaces;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using Sharedx.Infra.Outbox.Services;
using SharedX.Core.Bus;
using SharedX.Core.Entities;
using SharedX.Core.Extensions;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.MarketData;
using SharedX.Core.Specs;
namespace MarketDataX.ServerApp.Consumer;
public class ConsumerSecurityApp : OutboxBackgroundService<SecurityEngine>, IHostedService
{
    private readonly ConnectionZmq _config;
    private readonly ILogger<ConsumerSecurityApp> _logger;
    private PullSocket _receiver;
    private readonly IMediatorHandler _mediator;
    private static Thread ThreadReceiverSecurity = null!;
    private readonly ISecurityCache _cache = null!;

    public ConsumerSecurityApp(ILogger<ConsumerSecurityApp> logger,
        IOptions<ConnectionZmq> options,
        ISecurityCache cache,
        IMediatorHandler mediator,
        IOutboxCache<SecurityEngine> outboxCache,
        IBus bus) : base(logger, outboxCache, bus)
    {
        _logger = logger;
        _config = options.Value;
        _cache = cache;
        _mediator = mediator;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando o Receiver Securities ZeroMQ...");

        ThreadReceiverSecurity = new Thread(() => ReceiverSecurity(cancellationToken));
        ThreadReceiverSecurity.Name = nameof(ThreadReceiverSecurity);
        ThreadReceiverSecurity.Start();
        
        return Task.CompletedTask;
    }

    protected Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    private void ReceiverSecurity(CancellationToken stoppingToken)
    {
        bool isConnected = false;

        do
        {
            try
            {
                _logger.LogInformation($"Receiver de Securities tentando conectar..{_config.SecurityToMarketData.Uri}");
                using (_receiver = new PullSocket(">"+_config.SecurityToMarketData.Uri))
                {
                    _logger.LogInformation("Receiver de Securities Conectado!!!");
                    isConnected = true;

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var message = _receiver.ReceiveMultipartBytes()[0];
                        var security = message.DeserializeFromByteArrayProtobuf<Security>();
                        _cache.UpsertSecurity(security);
                        

                        Thread.Sleep(10);
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

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Finalizando o consumer de marketdata ZeroMQ...");
        _receiver.Disconnect(_config.SecurityToMarketData.Uri);
        _logger.LogInformation("Consumer de marketdata ZeroMq...Finalizado!");

        return Task.CompletedTask;
    }
}
