using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using SharedX.Core.Extensions;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.Specs;
namespace MatchingX.ServerApp.Consumer;
public class ConsumerOrderApp : BackgroundService
{
    private readonly ILogger<ConsumerOrderApp> _logger;
    private PullSocket _receiver;
    private readonly ConnectionZmq _config;
    private readonly IMatchingReceiver _matchReceiver;
    private static Thread ThreadReceiverOrder= null!;

    public ConsumerOrderApp(ILogger<ConsumerOrderApp> logger,
        IOptions<ConnectionZmq> options
        ,IMatchingReceiver matchReceiver
        )
    {
        _logger = logger;
        _config = options.Value;
        _matchReceiver = matchReceiver;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando o Receiver Order ZeroMQ...");

        ThreadReceiverOrder = new Thread(() => ReceiverOrder(cancellationToken));
        ThreadReceiverOrder.Name = nameof(ThreadReceiverOrder);
        ThreadReceiverOrder.Start();

        return Task.CompletedTask;
    }
    public async override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Finalizando o COnsumer Order ZeroMQ...");
        _receiver.Disconnect(_config.MatchingToMarketData.Uri);
        await base.StopAsync(cancellationToken);
    }

    private void ReceiverOrder(CancellationToken stoppingToken)
    {
        bool isConnected = false;
        
        do
        {
            try
            {
                _logger.LogInformation($"Receiver de ordens tentando conectar..{_config.OrderEngineToMatching.Uri}");
                using (_receiver = new PullSocket(_config.OrderEntryToOrderEngine.Uri))
                {
                    _logger.LogInformation("Receiver de ordens Conectado!!!");
                    isConnected = true;

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var message = _receiver.ReceiveMultipartBytes()[0];
                        var order = message.DeserializeFromByteArrayProtobuf<OrderEngine>();
                        _matchReceiver.ReceiveOrder(order);
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
        }while (!isConnected);
    }
}