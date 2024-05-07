using Microsoft.Extensions.Logging;
using QuickFix;
using SharedX.Core.Bus;
using SharedX.Core.Matching;
using SharedX.Core.Enums;
using MatchingX.Core.Repositories;
using SharedX.Core.Interfaces;
using MacthingX.Application.Interfaces;

namespace MacthingX.Application.Services;
public class MatchMarket : MatchBase, IMatchMarket
{
    public MatchMarket() : base()
    {
    }

    public void ReceiveOrder(OrderEng order)
    {
        this.AddOrder(order);
    }

    protected override void AddOrder(OrderEng order)
    {
        base.AddOrder(order);
    }

    protected override void CancelOrder(OrderEng orderToCancel)
    {
        base.CancelOrder(orderToCancel);
    }

    protected override void ReplaceOrder(OrderEng order)
    {
        base.ReplaceOrder(order);
    }

    protected override void MatchOrderMarket(OrderEng order)
    {
        if (!_buyOrders.TryGetValue(order.Symbol, out Dictionary<long, OrderEng> buyOrder))
            return;

        if (!_sellOrders.TryGetValue(order.Symbol, out Dictionary<long, OrderEng> sellOrder))
            return;
        
        bool cancelled = false;
        
        if (order.Side == SharedX.Core.Enums.SideTrade.Buy)
        {
            var orderToTrade = sellOrder.FirstOrDefault(sell=>sell.Value.Quantity == order.Quantity);
            if (!orderToTrade.Equals(default(KeyValuePair<long, OrderEng>)))
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
            if (!orderToTrade.Equals(default(KeyValuePair<long, OrderEng>)))
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
    protected override void MatchOrderLimit(OrderEng order)
    {
        throw new NotImplementedException();
    }
}