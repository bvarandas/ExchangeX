using MarketDataX.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using SharedX.Core.Bus;
using SharedX.Core.Extensions;
using SharedX.Core.Matching.MarketData;
using System.Diagnostics;

namespace MarketDataX.Infra.Client;
public class ConsumerMarketDataApp : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly ILogger<ConsumerMarketDataApp> _logger;
    private SubscriberSocket _subscriber;
    private readonly string _addressConnect;
    private readonly string _topic;
    private readonly IMarketDataChache _cache;
    private readonly IMediatorHandler _mediator;
    public ConsumerMarketDataApp(ILogger<ConsumerMarketDataApp> logger, IConfiguration config, IMarketDataChache cache, IMediatorHandler mediator)
    {
        _logger = logger;
        _config = config;
        _cache = cache;
        _mediator = mediator;   
        _addressConnect = _config["ConnectionStrings:MarquetDataZmqConsumer:uri"]!;
        _topic = _config["ConnectionStrings:MarketDataZmqConsumer:topic"]!;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando o MarketData Consumer do ZeroMQ...");
        var listExecutions = new List<MarketData>();
            
        using (_subscriber = new SubscriberSocket())
        {
            _subscriber.Connect(_addressConnect);
            _subscriber.Subscribe(_topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                var msg = _subscriber.ReceiveMultipartBytes(1);//.ReceiveFrameBytes();//.ReceiveFrameString();
                var marketData = msg[1].DeserializeFromByteArrayProtobuf<MarketData>();
                _cache.AddMarketData(marketData);
                Thread.Sleep(10);
            }
        }
        return Task.CompletedTask;
    }
    public async override Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Finalizando o consumer ZeroMQ...");
        _subscriber.Disconnect(_addressConnect);
        _logger.LogInformation("Consumer ZeroMq...Finalizado!");
    }
}
