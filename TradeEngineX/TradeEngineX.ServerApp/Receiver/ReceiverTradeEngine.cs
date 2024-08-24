using Microsoft.Extensions.Logging;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.DropCopy;
using TradeEngineX.Core.Interfaces;
namespace TradeEngineX.ServerApp.Receiver;
public class ReceiverTradeEngine : IReceiverEngine<TradeReport>
{
    private readonly ITradeEngineCache _tradeCache;
    private readonly ILogger<ReceiverTradeEngine> _logger;

    public ReceiverTradeEngine(ITradeEngineCache tradeCache, ILogger<ReceiverTradeEngine> logger)
    {
        _tradeCache = tradeCache;
        _logger = logger;
    }

    public void ReceiveEngine(TradeReport message, CancellationToken cancellationToken)
    {
        _tradeCache.UpsertTradeEngineAsync(message);
    }
}
