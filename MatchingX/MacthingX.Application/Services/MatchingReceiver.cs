using MacthingX.Application.Interfaces;
using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SharedX.Core.Enums;
using SharedX.Core.Matching;
namespace MacthingX.Application.Services;
public class MatchingReceiver : IMatchingReceiver
{
    public readonly ILogger<MatchingReceiver> _logger;
    public readonly IMatchLimit _matchLimit;
    public readonly IMatchMarket _matchMarket;
    public readonly IMatchStop _matchStop;
    public readonly IMatchStopLimit _matchStopLimit;
    public MatchingReceiver(ILogger<MatchingReceiver> logger,
        IMatchLimit matchLimit,
        IMatchMarket matchMarket,
        IMatchStop matchStop,
        IMatchStopLimit matchStopLimit)
    {
        _logger = logger;
        _matchLimit = matchLimit;
        _matchMarket = matchMarket; 
        _matchStop = matchStop;
        _matchStopLimit = matchStopLimit;
    }
    public void ReceiveOrder(Order order) 
    {
        _logger.LogInformation($"Chegou Order do tipo {order.OrderType.ToString()}");
        switch(order.OrderType)
        {
            case OrderType.Stop:
                _matchStop.ReceiveOrder(order);
                break;
            case OrderType.Market:
                _matchMarket.ReceiveOrder(order);
                break;
            case OrderType.StopLimit:
                _matchStopLimit.ReceiveOrder(order);
                break;
            case OrderType.Limit:
                _matchLimit.ReceiveOrder(order);
                break;
        }
    }
}