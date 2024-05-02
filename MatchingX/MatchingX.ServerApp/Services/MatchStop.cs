using MatchingX.Core.Repositories;
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
    public MatchStop(ILogger<MatchBase> logger, 
        IMediatorHandler mediator, 
        IApplication fix,
        IOrderRepository orderRepository,
        ITradeRepository tradeRepository) : base(logger, mediator, fix, orderRepository, tradeRepository)
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
        
        bool cancelled = false;
        if (order.Side == SideTrade.Buy)
        {
            var orderToTrade = sellOrder.FirstOrDefault(sell=>sell.Value.Quantity == order.Quantity);

            if (!orderToTrade.Equals(default(KeyValuePair<long, Order>)))
            {
                CreateTradeCapture(order, orderToTrade.Value);

                RemoveTradedOrders(ref buyOrder, ref sellOrder, order, orderToTrade.Value);
            }else
            {
                if (order.TimeInForce == TimeInForce.FOK)
                    RemoveCancelledOrders(ref sellOrder, order, ref cancelled);
            }
        }
        else if (order.Side == SideTrade.Sell)
        {
            var orderToTrade = buyOrder.FirstOrDefault(kvp => kvp.Value.Quantity == order.Quantity);

            if (!orderToTrade.Equals(default(KeyValuePair<long, Order>)))
            {
                CreateTradeCapture(orderToTrade.Value, order);

                RemoveTradedOrders(ref buyOrder, ref sellOrder, orderToTrade.Value, order);
            }
            else
            {
                if (order.TimeInForce == TimeInForce.FOK)
                    RemoveCancelledOrders(ref buyOrder, order, ref cancelled);
            }
        }
    }

    protected override void MatchOrderLimit(Order order)
    {
        throw new NotImplementedException();
    }
}
