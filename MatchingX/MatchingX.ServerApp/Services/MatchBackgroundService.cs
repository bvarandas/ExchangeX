using DropCopyX.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetMQ.Sockets;
using SharedX.Core.Bus;
using SharedX.Core.Specs;
using Microsoft.Extensions.Options;


namespace MatchingX.ServerApp.Services;
internal class MatchBackgroundService : BackgroundService
{
    private readonly ILogger<MatchBackgroundService> _logger;
    private readonly IMediatorHandler _mediator;
    private PublisherSocket _publisher;
    private readonly IExecutionReportChache _cache;
    private readonly IOptions<ConnectionZmq> _config;

    public MatchBackgroundService(ILogger<MatchBackgroundService> logger, 
        IOptions<ConnectionZmq> config, 
        IExecutionReportChache cache, 
        IMediatorHandler mediator)
    {
        _logger = logger;
        _config = config;
        _cache = cache;
        _mediator = mediator;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (_publisher = new PublisherSocket())
        {
            //_publisher.Bind("tcp://*:5555");
            _publisher.Bind(_config.Value.Publisher.Uri);

            while (stoppingToken.IsCancellationRequested)
            {




                await Task.Delay(5);
            }
        }
            
    }

    public async override Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Finalizando o publisher ZeroMQ...");
        _publisher.Disconnect(_config.Value.Publisher.Uri);
        await base.StopAsync(stoppingToken);
        _logger.LogInformation("Publisher ZeroMq...Finalizado!");
    }
}
