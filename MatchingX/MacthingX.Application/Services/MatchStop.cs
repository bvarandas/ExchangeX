using MacthingX.Application.Interfaces;
using MatchingX.Core.Repositories;
using Microsoft.Extensions.Logging;
using SharedX.Core.Bus;
using SharedX.Core.Enums;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching;
using System.Collections.Concurrent;
namespace MacthingX.Application.Services;
public class MatchStop : MatchBase, IMatchStop
{
    private readonly Thread ThreadOrdersStopPrice;
    private readonly ConcurrentQueue<OrderEng> QueueOrderStopPrice;
    public MatchStop() : base()
    {
        QueueOrderStopPrice = new ConcurrentQueue<OrderEng>();

        ThreadOrdersStopPrice = new Thread(new ThreadStart(OrderStopVerifyReachPrice));
        ThreadOrdersStopPrice.Name = nameof(OrderStopVerifyReachPrice);
        ThreadOrdersStopPrice.Start();
    }

    public void ReceiveOrder(OrderEng order)
    {
        this.AddOrder(order);
    }

    private void OrderStopVerifyReachPrice()
    {
        while (true)
        {
            if (QueueOrderStopPrice.TryDequeue(out OrderEng order))
            {
                if (!_lastPrice.TryGetValue(order.Symbol, out decimal price))
                {
                    QueueOrderStopPrice.Enqueue(order);
                    Thread.Sleep(10);
                    continue;
                }

                switch ((order.OrderType, order.Side))
                {
                    case (OrderType.Stop, SideTrade.Sell) when order.StopPrice >= price:
                    case (OrderType.Stop, SideTrade.Buy) when order.StopPrice <= price:
                        // Adicionar no book e mandar ordem market
                        base.AddOrder(order);
                        this.MatchOrderMarket(order);
                        break;
                    default:
                        QueueOrderStopPrice.Enqueue(order);
                        break;
                }
            }
            Thread.Sleep(10);
        }
    }
    protected override void AddOrder(OrderEng order)
    {
        QueueOrderStopPrice.Enqueue(order);
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
        if (order.Side == SideTrade.Buy)
        {
            var orderToTrade = sellOrder.FirstOrDefault(sell=>sell.Value.Quantity == order.Quantity);

            if (!orderToTrade.Equals(default(KeyValuePair<long, OrderEng>)))
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

            if (!orderToTrade.Equals(default(KeyValuePair<long, OrderEng>)))
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

    protected override void MatchOrderLimit(OrderEng order)
    {
        throw new NotImplementedException();
    }

    
}
