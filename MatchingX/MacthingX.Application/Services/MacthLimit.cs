using MacthingX.Application.Interfaces;
using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SharedX.Core.Bus;
using SharedX.Core.Enums;
using SharedX.Core.Matching.OrderEngine;
namespace MacthingX.Application.Services;
public class MatchLimit : MatchBase, IMatchLimit
{
    public MatchLimit(ILogger<MatchBase> logger, 
        IMediatorHandler bus, 
        IMatchingCache matchingCache,
        IMarketDataCache marketDataCache,
        ITradeOrderService tradeOrder) 
        : base(logger, bus, matchingCache, marketDataCache, tradeOrder )
    {
    }
    public void ReceiveOrder(OrderEngine order)
    {
        this.AddOrder(order);
    }

    protected override void AddOrder(OrderEngine order)
    {
        _tradeOrder.AddOrder(order);
    }
    protected override void CancelOrder(OrderEngine orderToCancel)
    {
        _tradeOrder.CancelOrder(orderToCancel);
    }
    protected override void ReplaceOrder(OrderEngine orderToReplace)
    {
        _tradeOrder.ReplaceOrder(orderToReplace);
    }

    protected override void MatchOrderLimit(OrderEngine order)
    {
        bool cancelled = false;
        if (order.Side == SideTrade.Buy)
        {
            var sellOrders = _matchingCache.GetSellOrderBySymbol(order.Symbol).Result.Value;
            var orderToTrade = sellOrders.FirstOrDefault(kvp => kvp.Value.Price <= order.Price &&
                                                               kvp.Value.Quantity == order.Quantity);
            if (!orderToTrade.Equals(default(KeyValuePair<long, OrderEngine>)))
            {
                _tradeOrder.CreateTradeCapture(order, orderToTrade.Value);
                _tradeOrder.RemoveTradedOrdersAsync(order, orderToTrade.Value);
            }else
            {
                if (order.TimeInForce == TimeInForce.FOK)
                    cancelled = _tradeOrder.RemoveCancelledOrdersAsync(order).Result;
            }
        }
        else if (order.Side == SideTrade.Sell)
        {
            var buyOrders = _matchingCache.GetBuyOrderBySymbol(order.Symbol).Result.Value;
            var orderToTrade = buyOrders.FirstOrDefault(kvp => kvp.Value.Price >= order.Price &&
                                                               kvp.Value.Quantity == order.Quantity);
            if (!orderToTrade.Equals(default(KeyValuePair<long, OrderEngine>)))
            {
                _tradeOrder.CreateTradeCapture(orderToTrade.Value, order);
                _tradeOrder.RemoveTradedOrdersAsync(orderToTrade.Value, order);
            }
            else
            {
                if (order.TimeInForce == TimeInForce.FOK)
                    cancelled = _tradeOrder.RemoveCancelledOrdersAsync(order).Result;
            }
        }
    }

    protected override void MatchOrderMarket(OrderEngine order)
    {
        throw new NotImplementedException();
    }
}