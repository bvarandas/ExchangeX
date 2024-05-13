using MacthingX.Application.Interfaces;
using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SharedX.Core.Bus;
using SharedX.Core.Enums;
using SharedX.Core.Matching.OrderEngine;
namespace MacthingX.Application.Services;
public class MatchLimit :  IMatchLimit, IMatch
{
    protected readonly ITradeOrderService _tradeOrder;
    protected readonly IMatchingCache _matchingCache;
    protected readonly IMarketDataCache _marketDataCache;
    public MatchLimit(ILogger<MatchLimit> logger, 
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
        _tradeOrder.AddOrder(order);
        return true;
    }
    public bool CancelOrder(OrderEngine orderToCancel)
    {
        _tradeOrder.CancelOrder(orderToCancel);
        return true;
    }
    public bool ReplaceOrder(OrderEngine orderToReplace)
    {
        _tradeOrder.ReplaceOrder(orderToReplace);
        return true;
    }
    public bool ModifyOrder(OrderEngine order)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> MatchBuyOrderAsync(OrderEngine order)
    {
        bool cancelled = false;
        var sellOrders = _matchingCache.GetSellOrderBySymbol(order.Symbol).Result.Value;
        var orderToTrade = new KeyValuePair<long, OrderEngine>();

        if (order.TimeInForce != TimeInForce.FOK)
        {
            orderToTrade = sellOrders
                .OrderBy(p => p.Value.Price)
                .FirstOrDefault(kvp => kvp.Value.Price <= order.Price && 
                 kvp.Value.LeavesQuantity >= order.Quantity);

        }else  if (order.TimeInForce == TimeInForce.FOK)
            orderToTrade = sellOrders
                .OrderBy(p => p.Value.Price)
                .FirstOrDefault(kvp => kvp.Value.Price <= order.Price && kvp.Value.Quantity == order.Quantity);


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
        var orderToTrade = new KeyValuePair<long, OrderEngine>();

        if (order.TimeInForce != TimeInForce.FOK)
        {
            orderToTrade = buyOrders
            .OrderByDescending(p => p.Value.Price)
            .FirstOrDefault(kvp => kvp.Value.Price >= order.Price && 
                            kvp.Value.LeavesQuantity >= order.Quantity);
        }
        else if (order.TimeInForce == TimeInForce.FOK)
        {
            orderToTrade = buyOrders
            .OrderByDescending(p => p.Value.Price)
            .FirstOrDefault(kvp => kvp.Value.Price >= order.Price &&
                            kvp.Value.LeavesQuantity == order.Quantity);
        }
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

    
}