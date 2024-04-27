using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuickFix;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using QuickFix.Transport;
using SharedX.Core.Specs;

namespace MatchingX.Infra.FixClientApp;

public class MatchingInitiator : BackgroundService 
{
    private readonly ILogger<MatchingInitiator> _logger;
    private readonly ITradeClientApp _tradeClient;
    private readonly SocketInitiator _initiator;
    public MatchingInitiator(ITradeClientApp tradeClient, ILogger<MatchingInitiator> logger)
    {
        _tradeClient = tradeClient;
        _logger = logger;

        SessionSettings settings = new SessionSettings(@"initiator.cfg");
        IMessageStoreFactory storeFactory = new FileStoreFactory(settings);
        ILogFactory logFactory = new ScreenLogFactory(settings);
        _initiator = new SocketInitiator(_tradeClient, storeFactory, settings, logFactory);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _initiator.Start();

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(5000, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _initiator?.Stop();
    }
}
