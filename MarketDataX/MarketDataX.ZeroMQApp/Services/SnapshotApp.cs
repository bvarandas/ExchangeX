using MarketDataX.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace MarketDataX.ServerApp.Services;
public class SnapshotApp : BackgroundService
{
    private readonly ILogger<SnapshotApp> _logger;
    private static Thread ThreadProcessSnapshot = null!;
    private static IMarketDataChache _marketDataCache = null!;
    private static ISecurityCache _securityCache = null!;
    public SnapshotApp(ILogger<SnapshotApp> logger, IMarketDataChache marketDataCache)
    {
        _logger = logger;
        _marketDataCache = marketDataCache;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando o Serviço de snapshot de marketdata");

        ThreadProcessSnapshot = new Thread(() => ReceiverSnapshot(cancellationToken));
        ThreadProcessSnapshot.Name = nameof(ThreadProcessSnapshot);
        ThreadProcessSnapshot.Start();

        return Task.CompletedTask;
    }

    private async void ReceiverSnapshot(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var securities = await _securityCache.GetSnapShotSecuritiesAsync();
            if (securities.IsSuccess)
            {
                //foreach ()
                //_marketDataCache.GetSnapShotMarketData()
            }

            Thread.Sleep(10000);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Finalizando o consumer de ordens do zeroMQ...");
        
        return Task.CompletedTask;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}