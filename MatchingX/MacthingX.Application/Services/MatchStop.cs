using MacthingX.Application.Interfaces;
using MassTransit;
using MatchingX.Core.Interfaces;
using MatchingX.Core.Repositories;
using MatchingX.Infra.Cache;
using Microsoft.Extensions.Logging;
using SharedX.Core.Bus;
using SharedX.Core.Enums;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching;
using SharedX.Core.Matching.OrderEngine;
using System.Collections.Concurrent;
namespace MacthingX.Application.Services;
public class MatchStop : MatchBase, IMatchStop
{
    private readonly Thread ThreadOrdersStopPrice;
    private readonly ConcurrentQueue<OrderEngine> QueueOrderStopPrice;
    public MatchStop(ILogger<MatchBase> logger, IMediatorHandler bus, IMatchingCache matchingCache) 
        : base(logger, bus, matchingCache)
    {
        QueueOrderStopPrice = new ConcurrentQueue<OrderEngine>();

        ThreadOrdersStopPrice = new Thread(new ThreadStart(OrderStopVerifyReachPrice));
        ThreadOrdersStopPrice.Name = nameof(OrderStopVerifyReachPrice);
        ThreadOrdersStopPrice.Start();
    }

    public void ReceiveOrder(OrderEngine order)
    {
        this.AddOrder(order);
    }

    private void OrderStopVerifyReachPrice()
    {
        while (true)
        {
            if (QueueOrderStopPrice.TryDequeue(out OrderEngine order))
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
    protected override void AddOrder(OrderEngine order)
    {
        switch(order.Execution )
        {
            case Execution.ToCancel when order.Side ==  SideTrade.Sell:

                break;
            case Execution.ToCancel when order.Side == SideTrade.Buy:

                break;
            case Execution.ToCancelReplace when order.Side == SideTrade.Sell:

                break;
            case Execution.ToCancelReplace when order.Side == SideTrade.Buy:

                break;
            case Execution.ToOpen:
                QueueOrderStopPrice.Enqueue(order);
                break;
        }
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
        if (order.Side == SideTrade.Buy)
        {
            var orderToTrade = sellOrder.FirstOrDefault(sell=>sell.Value.Quantity == order.Quantity);

            if (!orderToTrade.Equals(default(KeyValuePair<long, OrderEngine>)))
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

            if (!orderToTrade.Equals(default(KeyValuePair<long, OrderEngine>)))
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

    protected override void MatchOrderLimit(OrderEngine order)
    {
        throw new NotImplementedException();
    }

    
}
