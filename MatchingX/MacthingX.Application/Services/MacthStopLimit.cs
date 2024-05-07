using MacthingX.Application.Interfaces;
using MatchingX.Core.Repositories;
using Microsoft.Extensions.Logging;
using QuickFix;
using SharedX.Core.Bus;
using SharedX.Core.Enums;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching;
using System.Collections.Concurrent;
namespace MacthingX.Application.Services;
public class MatchStopLimit : MatchBase, IMatchStopLimit
{
    private readonly Thread ThreadOrdersStopLimitPrice;
    private readonly ConcurrentQueue<OrderEng> QueueOrderStopLimitPrice;
    public MatchStopLimit() : base()
    {
        QueueOrderStopLimitPrice = new ConcurrentQueue<OrderEng>();

        ThreadOrdersStopLimitPrice = new Thread(new ThreadStart(OrderStopLimitVerifyReachPrice));
        ThreadOrdersStopLimitPrice.Name = nameof(OrderStopLimitVerifyReachPrice);
        ThreadOrdersStopLimitPrice.Start();
    }
    public void ReceiveOrder(OrderEng order)
    {
        this.AddOrder(order);
    }

    protected void OrderStopLimitVerifyReachPrice()
    {
        while (true)
        {
            if (QueueOrderStopLimitPrice.TryDequeue(out OrderEng order))
            {
                if (!_lastPrice.TryGetValue(order.Symbol, out decimal price))
                {
                    QueueOrderStopLimitPrice.Enqueue(order);
                    Thread.Sleep(10);
                    continue;
                }

                switch ((order.OrderType, order.Side))
                {
                    case (OrderType.StopLimit, SideTrade.Sell) when order.StopPrice >= price:
                    case (OrderType.StopLimit, SideTrade.Buy) when order.StopPrice <= price:
                        // Adicionar no book e mandar ordem Limit
                        base.AddOrder(order);
                        this.MatchOrderLimit(order);
                        break;
                    default:
                        QueueOrderStopLimitPrice.Enqueue(order);
                        break;
                }
            }
            Thread.Sleep(10);
        }
    }

    protected override void MatchOrderLimit(OrderEng order)
    {
        if (_buyOrders.TryGetValue(order.Symbol, out Dictionary<long, OrderEng> buyOrder))
            return;

        if (_sellOrders.TryGetValue(order.Symbol, out Dictionary<long, OrderEng> sellOrder))
            return;
        
        bool cancelled = false;
        if (order.Side == SharedX.Core.Enums.SideTrade.Buy)
        {
            var orderToTrade = sellOrder.FirstOrDefault(kvp => kvp.Value.Price <= order.Price && 
                                                               kvp.Value.Quantity == order.Quantity);
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
            var orderToTrade = buyOrder.FirstOrDefault(kvp => kvp.Value.Price >= order.Price && 
                                                               kvp.Value.Quantity == order.Quantity);
            if (!orderToTrade.Equals(default(KeyValuePair<long, OrderEng>)))
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

    protected override void MatchOrderMarket(OrderEng order)
    {
        throw new NotImplementedException();
    }
}
