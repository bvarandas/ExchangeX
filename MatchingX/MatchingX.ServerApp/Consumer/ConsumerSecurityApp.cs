using MatchingX.Core.Interfaces;
using MatchingX.ServerApp.Publisher;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using SharedX.Core.Entities;
using SharedX.Core.Extensions;
using SharedX.Core.Specs;

namespace MatchingX.ServerApp.Consumer;
public class ConsumerSecurityApp : BackgroundService
{
    private readonly ILogger<PublisherMarketDataApp> _logger;
    private PullSocket _receiver;
    private readonly ConnectionZmq _config;
    private readonly IMatchingReceiver _matchReceiver;
    private static Thread ThreadReceiverOrder = null!;
    public ConsumerSecurityApp(ILogger<PublisherMarketDataApp> logger,
        IOptions<ConnectionZmq> options
        , IMatchingReceiver matchReceiver
        )
    {
        _logger = logger;
        _config = options.Value;
        _matchReceiver = matchReceiver;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
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
