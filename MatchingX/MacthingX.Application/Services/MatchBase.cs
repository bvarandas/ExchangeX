using SharedX.Core.Enums;
using Microsoft.Extensions.Logging;
using SharedX.Core.Bus;
using TradeReportTransType = SharedX.Core.Enums.TradeReportTransType;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;
using MatchingX.Core.Interfaces;
using MacthingX.Application.Interfaces;

namespace MacthingX.Application.Services;
public abstract class MatchBase :  IDisposable
{
    protected readonly ILogger<MatchBase> _logger;
    protected readonly IMediatorHandler Bus;
    
    private readonly IOrderRepository _orderRepository;
    protected readonly IMatchingCache _matchingCache;
    protected readonly IMarketDataCache _marketDataCache;
    protected readonly ITradeOrderService _tradeOrder;
    protected MatchBase(ILogger<MatchBase> logger, 
        IMediatorHandler bus, 
        IMatchingCache matchingCache, 
        IMarketDataCache marketDataCache,
        ITradeOrderService tradeOrder)
    {
        _logger = logger;
        Bus = bus;

        _matchingCache = matchingCache;
        _marketDataCache = marketDataCache;
        _tradeOrder = tradeOrder;

        this.LoadOrdersOnRestart();
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
    
    protected abstract void AddOrder(OrderEngine order);
    protected abstract void ReplaceOrder(OrderEngine order);
    protected abstract void CancelOrder(OrderEngine orderToCancel);
    protected abstract void MatchOrderLimit(OrderEngine order);
    protected abstract void MatchOrderMarket(OrderEngine order);
    public void Dispose()
    {
        
    }
}