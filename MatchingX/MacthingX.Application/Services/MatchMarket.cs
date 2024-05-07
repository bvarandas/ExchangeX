using Microsoft.Extensions.Logging;
using SharedX.Core.Bus;
using SharedX.Core.Enums;
using MacthingX.Application.Interfaces;
using SharedX.Core.Matching.OrderEngine;
using MatchingX.Core.Interfaces;
namespace MacthingX.Application.Services;
public class MatchMarket : MatchBase, IMatchMarket
{
    public MatchMarket(ILogger<MatchBase> logger, IMediatorHandler bus, IMatchingCache matchingCache) 
        : base(logger, bus, matchingCache)
    {
    }
    public void ReceiveOrder(OrderEngine order)
    {
        this.AddOrder(order);
    }
    protected override void AddOrder(OrderEngine order)
    {
        base.AddOrder(order);
    }
    protected override void CancelOrder(OrderEngine orderToCancel)
    {
        base.CancelOrder(orderToCancel);
    }

    protected override void ReplaceOrder(OrderEngine order)
    {
        base.ReplaceOrder(order);
    }

    protected override void MatchOrderMarket(OrderEngine order)
    {
        if (!_matchingCache.TryGetBuyOrders(order.Symbol, out Dictionary<long, OrderEngine> buyOrder))
            return;

        if (!_matchingCache.TryGetSellOrders(order.Symbol, out Dictionary<long, OrderEngine> sellOrder))
            return;
        
        bool cancelled = false;
        
        if (order.Side == SharedX.Core.Enums.SideTrade.Buy)
        {
            var orderToTrade = sellOrder.FirstOrDefault(sell=>sell.Value.Quantity == order.Quantity);
            if (!orderToTrade.Equals(default(KeyValuePair<long, OrderEngine>)))
            {
                CreateTradeCapture(order, orderToTrade.Value);

                RemoveTradedOrders(ref buyOrder, ref sellOrder, order, orderToTrade.Value);
            }else
            {
                if (order.TimeInForce == TimeInForce.FOK) 
                    RemoveCancelledOrders(ref buyOrder, order, ref cancelled);
            }
        }
        else if (order.Side == SharedX.Core.Enums.SideTrade.Sell)
        {
            var orderToTrade = buyOrder.FirstOrDefault(buy => buy.Value.Quantity == order.Quantity);
            if (!orderToTrade.Equals(default(KeyValuePair<long, OrderEngine>)))
            {
                CreateTradeCapture(orderToTrade.Value, order);

                RemoveTradedOrders(ref buyOrder, ref sellOrder, orderToTrade.Value, order);
            }else
            {
                if (order.TimeInForce == TimeInForce.FOK)
                    RemoveCancelledOrders(ref sellOrder, order, ref cancelled);
            }
        }
    }
    protected override void MatchOrderLimit(OrderEngine order)
    {
        throw new NotImplementedException();
    }
}