using Microsoft.Extensions.Logging;
using QuickFix;
using SharedX.Core.Bus;
using SharedX.Core.Enums;
using SharedX.Core.Matching;
using System.Collections.Concurrent;
namespace MacthingX.FixApp.Services;
internal class MacthStopLimit : MatchBase
{
    private readonly Thread ThreadOrdersStopLimitPrice;
    private readonly ConcurrentQueue<Order> QueueOrderStopLimitPrice;
    public MacthStopLimit(ILogger<MatchBase> logger, IMediatorHandler mediator, IApplication fix) : base(logger, mediator, fix)
    {
        QueueOrderStopLimitPrice = new ConcurrentQueue<Order>();

        ThreadOrdersStopLimitPrice = new Thread(new ThreadStart(OrderStopLimitVerifyReachPrice));
        ThreadOrdersStopLimitPrice.Name = nameof(OrderStopLimitVerifyReachPrice);
        ThreadOrdersStopLimitPrice.Start();
    }
    protected void OrderStopLimitVerifyReachPrice()
    {
        while (true)
        {
            if (!QueueOrderStopLimitPrice.TryDequeue(out Order order))
                continue;

            if (!_lastPrice.TryGetValue(order.Symbol, out decimal price))
            {
                QueueOrderStopLimitPrice.Enqueue(order);
                continue;
            }
            
            switch ((order.OrderType, order.Side))
            {
                case (OrderType.StopLimit, SideTrade.Sell) when order.Price >= price:
                case (OrderType.StopLimit, SideTrade.Buy) when order.Price <= price:
                    // Adicionar no book e mandar ordem Limit
                    base.AddOrder(order);
                    this.MatchOrderLimit(order);
                    break;
                default:
                    QueueOrderStopLimitPrice.Enqueue(order);
                    break;
            }
            
            Thread.Sleep(10);
        }
    }

    protected override void MatchOrderLimit(Order order)
    {
        if (_buyOrders.TryGetValue(order.Symbol, out Dictionary<long, Order> buyOrder))
            return;

        if (_sellOrders.TryGetValue(order.Symbol, out Dictionary<long, Order> sellOrder))
            return;

        if (order.Side == SharedX.Core.Enums.SideTrade.Buy)
        {
            var orderToTrade = sellOrder.FirstOrDefault(kvp => kvp.Value.Price <= order.Price);
            if (!orderToTrade.Equals(default(KeyValuePair<long, Order>)))
            {
                CreateTrade(order, orderToTrade.Value);

                RemoveTradedOrders(ref buyOrder, ref sellOrder, order, orderToTrade.Value);
            }
        }
        else if (order.Side == SharedX.Core.Enums.SideTrade.Sell)
        {
            var orderToTrade = sellOrder.FirstOrDefault(kvp => kvp.Value.Price >= order.Price);
            if (!orderToTrade.Equals(default(KeyValuePair<long, Order>)))
            {
                CreateTrade(order, orderToTrade.Value);

                RemoveTradedOrders(ref buyOrder, ref sellOrder, order, orderToTrade.Value);
            }
        }
    }

    protected override void MatchOrderMarket(Order order)
    {
        throw new NotImplementedException();
    }
}
