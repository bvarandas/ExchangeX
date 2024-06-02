using MassTransit;
using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using Sharedx.Infra.Outbox.Services;
using SharedX.Core.Entities;
using SharedX.Core.Extensions;
using SharedX.Core.Interfaces;
using SharedX.Core.Specs;
namespace MatchingX.ServerApp.Consumer;
public class ConsumerSecurityApp : OutboxBackgroundService<SecurityEngine>, IHostedService
{
    private readonly ILogger<OutboxBackgroundService<SecurityEngine>> _logger;
    private PullSocket _receiver;
    private readonly ConnectionZmq _config;
    private readonly IMatchingReceiver _matchReceiver;
    private static Thread ThreadReceiverSecurity = null!;
    public ConsumerSecurityApp(ILogger<OutboxBackgroundService<SecurityEngine>> logger,
        IOptions<ConnectionZmq> options, 
        IMatchingReceiver matchReceiver,
        IOutboxCache<SecurityEngine> outboxCache,
        IBus bus
        ):base(logger, outboxCache, bus)
    {
        _logger = logger;
        _config = options.Value;
        _matchReceiver = matchReceiver;
    }

    protected Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando o Receiver Order ZeroMQ...");

        ThreadReceiverSecurity = new Thread(() => ReceiverSecurity(cancellationToken));
        ThreadReceiverSecurity.Name = nameof(ThreadReceiverSecurity);
        ThreadReceiverSecurity.Start();

        return Task.CompletedTask;
    }
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Finalizando o COnsumer Order ZeroMQ...");
        _receiver.Disconnect(_config.MatchingToMarketData.Uri);
        
        return Task.CompletedTask;
    }

    private void ReceiverSecurity(CancellationToken stoppingToken)
    {
        bool isConnected = false;

        do
        {
            try
            {
                _logger.LogInformation($"Receiver de ordens tentando conectar..{_config.SecurityToMatching.Uri}");
                using (_receiver = new PullSocket(_config.SecurityToMatching.Uri))
                {
                    _logger.LogInformation("Receiver de ordens Conectado!!!");
                    isConnected = true;

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var message = _receiver.ReceiveMultipartBytes()[0];
                        var security = message.DeserializeFromByteArrayProtobuf<SecurityEngine>();
                        _matchReceiver.ReceiveSecurity(security);
                        DeleteOutboxCacheAsync(security, long.Parse(security.SecurityID));

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
}
