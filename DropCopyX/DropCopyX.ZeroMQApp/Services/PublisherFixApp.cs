using DropCopyX.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuickFix;
using TradeReportTransType = QuickFix.Fields.TradeReportTransType;

namespace DropCopyX.ServerApp.Services;
internal class PublisherFixApp : BackgroundService
{
    private readonly ILogger<PublisherFixApp> _logger;
    private readonly ThreadedSocketAcceptor _acceptor;
    private readonly IFixServerApp _appFix;
    
    
    public PublisherFixApp(ILogger<PublisherFixApp> logger, IFixServerApp app)
    {
        _logger = logger;
        SessionSettings settings = new SessionSettings(@"acceptor.cfg");
        //IApplication application = app;
        _appFix = app;
        ScreenLogFactory logFactory = new ScreenLogFactory(settings);

        IMessageStoreFactory messageFactory = new FileStoreFactory(settings);

        _acceptor = new
            ThreadedSocketAcceptor(_appFix, messageFactory, settings, logFactory);

    }
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando o Sender Report FIX...");

        return Task.CompletedTask;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Finalizando o publisher FIX...");
        _acceptor.Stop();
        _logger.LogInformation("Publisher FIX...Finalizado!");

        return base.StopAsync(stoppingToken);
    }
}