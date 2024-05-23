using MatchingX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.Enums;
using SharedX.Core.Interfaces;
namespace MacthingX.Application.Services;
public class MatchContextStrategy : IMatchContextStrategy
{
    private readonly IEnumerable<IMatch> _matchList;
    private readonly IOrderRepository _orderRepository;
    private IMatchingCache _matchingCache;
    private IMatch _actualMatch;
         
    
    public MatchContextStrategy(IEnumerable<IMatch> matchList, 
        IOrderRepository orderRepository, 
        IMatchingCache matchingCache)
    {
        this._matchList = matchList;
        this._orderRepository = orderRepository;
        this._matchingCache = matchingCache;

        LoadOrdersOnRestart();
    }

    public void SetStrategy(string strategyName)
    {
        var instance = _matchList.FirstOrDefault(x =>
            x.Name.Equals(strategyName, StringComparison.InvariantCultureIgnoreCase));

        _actualMatch = instance!;
    }

    private void LoadOrdersOnRestart()
    {
        var ordersDb = _orderRepository.GetOrdersOnRestartAsync(default(CancellationToken));

        var buyOrders = ordersDb.Result.Where(o => o.Side == SideTrade.Buy);
        var sellOrders = ordersDb.Result.Where(o => o.Side == SideTrade.Sell);

        foreach (var order in buyOrders)
            _matchingCache.UpsertBuyOrder(order);

        foreach (var order in sellOrders)
            _matchingCache.UpsertSellOrder(order);
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