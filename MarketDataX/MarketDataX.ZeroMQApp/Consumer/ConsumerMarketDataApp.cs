using MarketDataX.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using SharedX.Core.Bus;
using SharedX.Core.Extensions;
using SharedX.Core.Matching.MarketData;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.Specs;
namespace MarketDataX.ServerApp.Consumer;
public class ConsumerMarketDataApp : BackgroundService
{
    private readonly ConnectionZmq _config;
    private readonly ILogger<ConsumerMarketDataApp> _logger;
    private PullSocket _receiver;
    private readonly IMarketDataChache _cache;
    private readonly IMediatorHandler _mediator;
    private static Thread ThreadReceiverMarketData = null!;
    public ConsumerMarketDataApp(ILogger<ConsumerMarketDataApp> logger, 
        IOptions<ConnectionZmq> options, 
        IMarketDataChache cache, 
        IMediatorHandler mediator)
    {
        _logger = logger;
        _config = options.Value;
        _cache = cache;
        _mediator = mediator;   
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando o Receiver Marketdata ZeroMQ...");

        ThreadReceiverMarketData = new Thread(() => ReceiverMarketData(cancellationToken));
        ThreadReceiverMarketData.Name = nameof(ThreadReceiverMarketData);
        ThreadReceiverMarketData.Start();
        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
    public async override Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Finalizando o consumer de marketdata ZeroMQ...");
        _receiver.Disconnect(_config.MatchingToMarketData.Uri);
        _logger.LogInformation("Consumer de marketdata ZeroMq...Finalizado!");
    }

    private void ReceiverMarketData(CancellationToken stoppingToken)
    {
        bool isConnected = false;

        do
        {
            try
            {
                _logger.LogInformation($"Receiver de marketdata tentando conectar..{_config.OrderEngineToMatching.Uri}");
                using (_receiver = new PullSocket(_config.OrderEngineToMatching.Uri))
                {
                    _logger.LogInformation("Receiver de marketdata Conectado!!!");
                    isConnected = true;

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var message = _receiver.ReceiveMultipartBytes()[0];
                        var marketData = message.DeserializeFromByteArrayProtobuf<MarketData>();
                        _cache.AddMarketData(marketData);
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