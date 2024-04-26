using Microsoft.Extensions.Logging;
using QuickFix;
using SharedX.Core.Bus;
using SharedX.Core.Matching;

namespace MacthingX.FixApp.Services;

internal class MatchMarket : MatchBase
{
    public MatchMarket(ILogger<MatchBase> logger, IMediatorHandler mediator, IApplication fix) : base(logger, mediator, fix)
    {
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

    protected override void MatchOrderMarket(Order order)
    {
        if (!_buyOrders.TryGetValue(order.Symbol, out Dictionary<long, Order> buyOrder))
            return;

        if (_sellOrders.TryGetValue(order.Symbol, out Dictionary<long, Order> sellOrder))
            return;

        if (order.Side == SharedX.Core.Enums.SideTrade.Buy)
        {
            var orderToTrade = sellOrder.FirstOrDefault();
            if (!orderToTrade.Equals(default(KeyValuePair<long, Order>)))
            {
                CreateTrade(order, orderToTrade.Value);

                RemoveTradedOrders(ref buyOrder, ref sellOrder, order, orderToTrade.Value);
            }
        }
        else if (order.Side == SharedX.Core.Enums.SideTrade.Sell)
        {
            var orderToTrade = sellOrder.FirstOrDefault();
            if (!orderToTrade.Equals(default(KeyValuePair<long, Order>)))
            {
                CreateTrade(order, orderToTrade.Value);

                RemoveTradedOrders(ref buyOrder, ref sellOrder, order, orderToTrade.Value);
            }
        }
    }
    protected override void MatchOrderLimit(Order order)
    {
        throw new NotImplementedException();
    }
}