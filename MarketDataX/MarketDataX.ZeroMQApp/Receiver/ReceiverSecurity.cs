using MarketDataX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.MarketData;
namespace MarketDataX.ServerApp.Receiver;
public class ReceiverSecurity : IReceiverEngine<Security>
{
    private readonly ILogger<ReceiverSecurity> _logger;
    private readonly ISecurityCache _cache = null!;

    public ReceiverSecurity(ILogger<ReceiverSecurity> logger, ISecurityCache cache)
    {
        _logger = logger;
        _cache = cache;
    }
    public void ReceiveEngine(Security message, CancellationToken cancellationToken)
    {
        _cache.UpsertSecurity(message);
    }
}