using Microsoft.Extensions.Logging;
using QuickFix;
using SharedX.Core.Bus;
using SharedX.Core.Enums;
using SharedX.Core.Matching;
using System.Collections.Concurrent;
namespace MacthingX.FixApp.Services;
internal class MatchStop : MatchBase
{
    private readonly Thread ThreadOrdersStopPrice;
    private readonly ConcurrentQueue<Order> QueueOrderStopPrice;
    public MatchStop(ILogger<MatchBase> logger, IMediatorHandler mediator, IApplication fix) : base(logger, mediator, fix)
    {
        QueueOrderStopPrice = new ConcurrentQueue<Order>();

        ThreadOrdersStopPrice = new Thread(new ThreadStart(OrderStopVerifyReachPrice));
        ThreadOrdersStopPrice.Name = nameof(OrderStopVerifyReachPrice);
        ThreadOrdersStopPrice.Start();
    }

    private void OrderStopVerifyReachPrice()
    {
        while (true)
        {
            if (!QueueOrderStopPrice.TryDequeue(out Order order))
                continue;

            if (!_lastPrice.TryGetValue(order.Symbol, out decimal price))
            {
                QueueOrderStopPrice.Enqueue(order);
                return;
            }

            switch ((order.OrderType, order.Side))
            {
                case (OrderType.Stop, SideTrade.Sell) when order.Price >= price:
                case (OrderType.Stop, SideTrade.Buy) when order.Price <= price:
                    // Adicionar no book e mandar ordem market
                    base.AddOrder(order);
                    this.MatchOrderMarket(order);
                    break;
                default:
                    QueueOrderStopPrice.Enqueue(order);
                    break;
            }

            Thread.Sleep(10);
        }
    }
    protected override void AddOrder(Order order)
    {
        QueueOrderStopPrice.Enqueue(order);
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
        if (!_sellOrders.TryGetValue(order.Symbol, out Dictionary<long, Order> sellOrder))
            return;
            
        if (order.Side == SideTrade.Buy)
        {
            var orderToTrade = sellOrder.FirstOrDefault();

            if (!orderToTrade.Equals(default(KeyValuePair<long, Order>)))
            {
                CreateTrade(order, orderToTrade.Value);

                RemoveTradedOrders(ref buyOrder, ref sellOrder, order, orderToTrade.Value);
            }
        }
        else if (order.Side == SideTrade.Sell)
        {
            var orderToTrade = sellOrder.FirstOrDefault(kvp => kvp.Value.Price >= order.Price);

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
