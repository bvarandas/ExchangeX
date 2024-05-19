using MarketDataX.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuickFix;
namespace MarketDataX.ServerApp.Services;
internal class PublisherFixApp : BackgroundService
{
    private readonly ILogger<PublisherFixApp> _logger;
    private readonly ThreadedSocketAcceptor _acceptor;
    private readonly IFixServerApp app;

    public PublisherFixApp(ILogger<PublisherFixApp> logger, IFixServerApp app)
    {
        _logger = logger;
        SessionSettings settings = new SessionSettings(@"acceptor.cfg");
        IApplication application = app;
        ScreenLogFactory logFactory = new ScreenLogFactory(settings);

        IMessageStoreFactory messageFactory = new FileStoreFactory(settings);

        _acceptor = new
            ThreadedSocketAcceptor(application, messageFactory, settings, logFactory);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando o Fix acceptor...");

        _acceptor.Start();

        return Task.CompletedTask;
    }

    public async override Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Finalizando o publisher FIX...");
        _acceptor.Stop();

        _logger.LogInformation("Publisher FIX...Finalizado!");
    }
}