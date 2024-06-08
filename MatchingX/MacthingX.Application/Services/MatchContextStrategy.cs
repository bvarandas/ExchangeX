using MatchingX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.Interfaces;
using SecurityX.Core.Interfaces;
using SharedX.Core.Entities;

namespace MacthingX.Application.Services;
public class MatchContextStrategy : IMatchContextStrategy
{
    private readonly IEnumerable<IMatch> _matchList;
    private readonly IOrderRepository _orderRepository;
    private readonly ISecurityCache _securityCache;
    private IMatchingCache _matchingCache;
    private IMatch _actualMatch;
         
    
    public MatchContextStrategy(IEnumerable<IMatch> matchList, 
        IOrderRepository orderRepository, 
        IMatchingCache matchingCache,
        ISecurityCache securityCache)
    {
        this._matchList = matchList;
        this._orderRepository = orderRepository;
        this._matchingCache = matchingCache;
        this._securityCache = securityCache;

        LoadOrdersOnRestart();
    }

    public void SetStrategy(string strategyName)
    {
        var instance = _matchList.FirstOrDefault(x =>
            x.Name.Equals(strategyName, StringComparison.InvariantCultureIgnoreCase));

        _actualMatch = instance!;
    }

    private async void LoadOrdersOnRestart()
    {
        var ordersDb = await _orderRepository.GetOrdersOnRestartAsync(default(CancellationToken));
        
        if (ordersDb is not null)
        foreach (var order in ordersDb)
        {

        }

        //var buyOrders = ordersDb.Where(o => o.Side == SideTrade.Buy);
        //var sellOrders = ordersDb.Where(o => o.Side == SideTrade.Sell);

        //foreach (var order in sellOrders.ToList())
        //    await _matchingCache.UpsertSellOrder(order);

        //foreach (var order in buyOrders.ToList())
        //    await _matchingCache.UpsertBuyOrder(order);
    }

    public void ReceivedOrder(OrderEngine order)
    {
        this._actualMatch.ReceiveOrder(order);
    }
    public async Task<bool> MatchOrderAsync(OrderEngine order)
    {
        bool result = false;
        result  = await this._actualMatch.MatchOrderAsync(order);
        return result;
    }

    public async Task<bool> CancelOrderAsync(OrderEngine order)
    {
        bool result = false;
        result = this._actualMatch.CancelOrder(order);
        return result;
    }
}