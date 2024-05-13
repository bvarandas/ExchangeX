using Microsoft.Extensions.Logging;
using SharedX.Core.Bus;
using SharedX.Core.Enums;
using MacthingX.Application.Interfaces;
using SharedX.Core.Matching.OrderEngine;
using MatchingX.Core.Interfaces;

namespace MacthingX.Application.Services;
public class MatchMarket : IMatchMarket, IMatch
{
    protected readonly ITradeOrderService _tradeOrder;
    protected readonly IMatchingCache _matchingCache;
    protected readonly IMarketDataCache _marketDataCache;
    public MatchMarket(ILogger<MatchMarket> logger, 
        IMediatorHandler bus, 
        IMatchingCache matchingCache,
        IMarketDataCache marketDataCache,
        ITradeOrderService tradeOrder) 
    {
        _tradeOrder = tradeOrder;
        _matchingCache = matchingCache;
        _marketDataCache = marketDataCache;
    }
    public void ReceiveOrder(OrderEngine order)
    {
        this.AddOrder(order);
    }
    public bool AddOrder(OrderEngine order)
    {
        return _tradeOrder.AddOrder(order);
    }
    public bool CancelOrder(OrderEngine orderToCancel)
    {
        return _tradeOrder.CancelOrder(orderToCancel);
    }

    public bool ReplaceOrder(OrderEngine orderToReplace)
    {
        return _tradeOrder.ReplaceOrder(orderToReplace);
    }

    public async Task<bool> MatchBuyOrderAsync(OrderEngine order)
    {
        bool cancelled = false;
        var sellOrders = _matchingCache.GetSellOrderBySymbol(order.Symbol).Result.Value;
        var orderToTrade = sellOrders
            .OrderBy(p => p.Value.Price)
            .FirstOrDefault(sell => sell.Value.Quantity == order.Quantity);

        if (!orderToTrade.Equals(default(KeyValuePair<long, OrderEngine>)))
        {
            _tradeOrder.CreateTradeCapture(order, orderToTrade.Value);
            await _tradeOrder.RemoveTradedOrdersAsync(order, orderToTrade.Value);
        }
        else
        {
            if (order.TimeInForce == TimeInForce.FOK)
                cancelled = _tradeOrder.RemoveCancelledOrdersAsync(order).Result;
        }
        return true;
    }

    public async Task<bool> MatchSellOrderAsync(OrderEngine order)
    {
        bool cancelled = false;
        var buyOrders = _matchingCache.GetBuyOrderBySymbol(order.Symbol).Result.Value;
        var orderToTrade = buyOrders
            .OrderByDescending(p=>p.Value.Price)
            .FirstOrDefault(buy => buy.Value.Quantity == order.Quantity);

        if (!orderToTrade.Equals(default(KeyValuePair<long, OrderEngine>)))
        {
            _tradeOrder.CreateTradeCapture(orderToTrade.Value, order);
            await _tradeOrder.RemoveTradedOrdersAsync(orderToTrade.Value, order);
        }
        else
        {
            if (order.TimeInForce == TimeInForce.FOK)
                cancelled = _tradeOrder.RemoveCancelledOrdersAsync(order).Result;
        }
        return true;
    }

    public bool ModifyOrder(OrderEngine order)
    {
        throw new NotImplementedException();
    }
}