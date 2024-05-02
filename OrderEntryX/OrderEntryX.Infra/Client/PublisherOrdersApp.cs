using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ.Sockets;
using SharedX.Core.Matching.MarketData;
using SharedX.Core.Specs;
namespace OrderEntryX.Infra.Client;
public class PublisherOrdersApp : BackgroundService
{
    private readonly ConnectionZmq _config;
    private readonly ILogger<PublisherOrdersApp> _logger;
    private PublisherSocket _publisher;
    public PublisherOrdersApp(ILogger<PublisherOrdersApp> logger, 
        IOptions<ConnectionZmq> options)
    {
        _logger = logger;
        _config = options.Value;
    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando o MarketData Consumer do ZeroMQ...");
        var listExecutions = new List<MarketData>();
            
        using (_publisher = new PublisherSocket())
        {
            _publisher.Bind(_config.PublisherOrders.Uri);
            //var timer = new Stopwatch();
            //timer.Start();

            while (!stoppingToken.IsCancellationRequested)
            {
                //var message = 

                //_publisher
                //    .SendMoreFrame(_config.Publisher.Topic[])
                //    .SendMultipartBytes();

                Thread.Sleep(10);
            }
        }
        return Task.CompletedTask;
    }

    public async override Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Finalizando o publisher ZeroMQ...");
        _publisher.Disconnect(_config.PublisherOrders.Uri);
        _logger.LogInformation("Publisher ZeroMq...Finalizado!");
        await base.StopAsync(stoppingToken);
     }
}