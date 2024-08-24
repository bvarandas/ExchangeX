using MarketDataX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.MarketData;
namespace MarketDataX.ServerApp.Receiver;
public class ReceiverMarketData : IReceiverEngine<MarketData>
{
    private readonly ILogger<ReceiverMarketData> _logger;
    private readonly IMarketDataChache _cache;

    public ReceiverMarketData(ILogger<ReceiverMarketData> logger,
        IMarketDataChache cache)
    {
        _logger = logger;
        _cache = cache;
    }
    public void ReceiveEngine(MarketData message, CancellationToken cancellationToken)
    {
        _cache.AddMarketDataIncremental(message);
        _cache.AddMarketDataBook(message);
    }
}