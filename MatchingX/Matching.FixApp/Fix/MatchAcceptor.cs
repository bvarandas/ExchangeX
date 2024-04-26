using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuickFix;

namespace MatchinX.API.Fix;
public class MatchAcceptor : BackgroundService
{
    private const string HttpServerPrefix = "http://127.0.0.1:5080/";
    private readonly ILogger<MatchAcceptor> _logger;
    private readonly IApplication app;
    public MatchAcceptor(ILogger<MatchAcceptor> logger, IApplication app)
    {
        _logger = logger;


        SessionSettings settings = new SessionSettings(@"acceptor.cfg");
        IApplication application = app;

        //FileStoreFactory storeFactory = new FileStoreFactory(settings);
        ScreenLogFactory logFactory = new ScreenLogFactory(settings);
        
        IMessageStoreFactory messageFactory = new FileStoreFactory(settings); 
        
        ThreadedSocketAcceptor acceptor= new 
            ThreadedSocketAcceptor(application, messageFactory, settings, logFactory);

        
        acceptor.Start();
        
        while (true)
        {
            Console.WriteLine("Aguardando mensagens");
            Thread.Sleep(10);
        }
        
        acceptor.Stop();
    }
    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Aguardando mensagens Command...");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(50, stoppingToken);
        }
    }

    public async override Task StopAsync(CancellationToken stoppingToken)
    {
        //_channel.Close();
    }
}
