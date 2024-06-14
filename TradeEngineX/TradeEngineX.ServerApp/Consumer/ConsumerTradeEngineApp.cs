using MassTransit.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using SharedX.Core.Bus;
using SharedX.Core.Matching.DropCopy;
using SharedX.Core.Specs;
using TradeEngineX.Core.Interfaces;
using SharedX.Core.Extensions; 
namespace TradeEngineX.ServerApp.Consumer;
public class ConsumerTradeEngineApp : BackgroundService
{
    private readonly ILogger<ConsumerTradeEngineApp> _logger;
    private PullSocket _receiver;
    private readonly ConnectionZmq _config;
    private static Thread ThreadReceiverTrade = null!;
    private readonly IMediatorHandler _mediator;
    private readonly ITradeEngineCache _tradeCache;
    public ConsumerTradeEngineApp(ILogger<ConsumerTradeEngineApp> logger,
        IOptions<ConnectionZmq> options,
        IMediatorHandler mediator,
        ITradeEngineCache tradeCache)
    {
        _logger = logger;
        _tradeCache = tradeCache;
        _mediator = mediator;
        _config = options.Value;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing o Trade receiver ZeroMQ...");
        ThreadReceiverTrade = new Thread(() => ReceiverTrades(cancellationToken));
        ThreadReceiverTrade.Name = nameof(ThreadReceiverTrade);
        ThreadReceiverTrade.Start();

        return Task.CompletedTask;
    }

    private void ReceiverTrades(CancellationToken stoppingToken)
    {
        bool isConnected = false;

        do
        {
            try
            {
                _logger.LogInformation("Iniciando o Trade receiver do ZeroMQ...");

                using (_receiver = new PullSocket(">" + _config.MatchingToTradeEngine.Uri))
                {
                    _logger.LogInformation("Receiver de Trade Conectado!!!");
                    isConnected = true;

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var msg = _receiver.ReceiveMultipartBytes();
                        var trade = msg[1].DeserializeFromByteArrayProtobuf<TradeReport>();

                        _tradeCache.UpsertTradeEngineAsync(trade);

                        Thread.Sleep(10);
                    }
                }
            }catch(Exception ex)
            {
                isConnected = false;
                _logger.LogError(ex.Message, ex);
            }
        }while (!isConnected);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Finishing the Consumer ZeroMQ...");
        _receiver.Disconnect(_config.OrderEntryToOrderEngine.Uri);
        _logger.LogInformation("Consumer ZeroMq...Finishing!");
        return base.StopAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}
