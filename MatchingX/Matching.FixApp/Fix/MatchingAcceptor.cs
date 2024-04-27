using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuickFix;

namespace MatchinX.API.Fix;
public class MatchingAcceptor : BackgroundService
{
    private const string HttpServerPrefix = "http://127.0.0.1:5080/";
    private readonly ILogger<MatchingAcceptor> _logger;
    private readonly IApplication app;
    private readonly ThreadedSocketAcceptor _acceptor;
    public MatchingAcceptor(ILogger<MatchingAcceptor> logger, IApplication app)
    {
        _logger = logger;


        SessionSettings settings = new SessionSettings(@"acceptor.cfg");
        IApplication application = app;

        //FileStoreFactory storeFactory = new FileStoreFactory(settings);
        ScreenLogFactory logFactory = new ScreenLogFactory(settings);
        
        IMessageStoreFactory messageFactory = new FileStoreFactory(settings); 
        
        _acceptor= new 
            ThreadedSocketAcceptor(application, messageFactory, settings, logFactory);
    }
    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando o Fix acceptor...");

        _acceptor.Start();

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(5000, stoppingToken);
        }
    }

    public async override Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Finalizando o Fix acceptor...");

        _acceptor.Stop();
        _logger.LogInformation("Fix acceptor...Finalizado!");
    }
}