using MacthingX.Application.Interfaces;
using MatchingX.Core.Repositories;
using Microsoft.Extensions.Logging;
using QuickFix;
using SharedX.Core.Bus;
using SharedX.Core.Enums;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching;
namespace MacthingX.Application.Services;
public class MatchLimit : MatchBase, IMatchLimit
{
    public MatchLimit() : base()
    {
    }
    public void ReceiveOrder(Order order)
    {
        this.AddOrder(order);
    }

    protected override void AddOrder(Order order)
    {
        base.AddOrder(order);
    }
    protected override void CancelOrder(Order orderToCancel)
    {
        base.CancelOrder(orderToCancel);
    }

    protected override void ReplaceOrder(Order order)
    {
        base.ReplaceOrder(order);
    }

    protected override void MatchOrderLimit(Order order)
    {
        if (!_buyOrders.TryGetValue(order.Symbol, out Dictionary<long, Order> buyOrder))
            return;

        if (!_sellOrders.TryGetValue(order.Symbol, out Dictionary<long, Order> sellOrder))
            return;

        bool cancelled = false;
        if (order.Side == SharedX.Core.Enums.SideTrade.Buy)
        {
            var orderToTrade = sellOrder.FirstOrDefault(kvp => kvp.Value.Price <= order.Price &&
                                                               kvp.Value.Quantity == order.Quantity);
            if (!orderToTrade.Equals(default(KeyValuePair<long,Order>)))
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
            var orderToTrade = buyOrder.FirstOrDefault(kvp => kvp.Value.Price >= order.Price &&
                                                               kvp.Value.Quantity == order.Quantity);
            if (!orderToTrade.Equals(default(KeyValuePair<long, Order>)))
            {
                CreateTradeCapture(orderToTrade.Value, order);
                RemoveTradedOrders(ref buyOrder, ref sellOrder, orderToTrade.Value, order);
            }
            else
            {
                if (order.TimeInForce == TimeInForce.FOK)
                    RemoveCancelledOrders(ref sellOrder, order, ref cancelled);
            }
        }
    }

    protected override void MatchOrderMarket(Order order)
    {
        throw new NotImplementedException();
    }

    
}