using MacthingX.Application.Interfaces;
using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SharedX.Core.Bus;
using SharedX.Core.Enums;
using SharedX.Core.Matching.OrderEngine;
using System.Collections.Concurrent;
namespace MacthingX.Application.Services;
public class MatchStopLimit : MatchBase, IMatchStopLimit
{
    private readonly Thread ThreadOrdersStopLimitPrice;
    private readonly ConcurrentQueue<OrderEngine> QueueOrderStopLimitPrice;
    private readonly ConcurrentDictionary<long, OrderEngine> DicOrdersToCancel;
    public MatchStopLimit(ILogger<MatchBase> logger, 
        IMediatorHandler bus, 
        IMatchingCache matchingCache, 
        IMarketDataCache marketDataCache,
        ITradeOrderService tradeOrder ) 
        : base(logger, bus, matchingCache, marketDataCache, tradeOrder)
    {
        QueueOrderStopLimitPrice = new ConcurrentQueue<OrderEngine>();

        ThreadOrdersStopLimitPrice = new Thread(new ThreadStart(OrderStopLimitVerifyReachPrice));
        ThreadOrdersStopLimitPrice.Name = nameof(OrderStopLimitVerifyReachPrice);
        ThreadOrdersStopLimitPrice.Start();

        DicOrdersToCancel = new ConcurrentDictionary<long, OrderEngine>();
    }

    protected override void AddOrder(OrderEngine order)
    {
        QueueOrderStopLimitPrice.Enqueue(order);
        
    }

    public void ReceiveOrder(OrderEngine order)
    {
        this.AddOrder(order);
    }

    protected override void ReplaceOrder(OrderEngine order)
    {
        _tradeOrder.ReplaceOrder(order);
    }

    protected override void CancelOrder(OrderEngine orderToCancel)
    {
        DicOrdersToCancel.TryAdd(orderToCancel.OrderID, orderToCancel);
        _tradeOrder.CancelOrder(orderToCancel);
    }

    protected async void OrderStopLimitVerifyReachPrice()
    {
        while (true)
        {
            if (QueueOrderStopLimitPrice.TryDequeue(out OrderEngine order))
            {
                if (DicOrdersToCancel.TryGetValue(order.OrderID, out OrderEngine orderFound))
                {
                    DicOrdersToCancel.Remove(order.OrderID, out OrderEngine orderRemoved);
                    continue;
                }

                var price =await _marketDataCache.GetPrice(order.Symbol);

                if (!price.Equals(0))
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
                        this.AddOrder(order);
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

    protected override void MatchOrderLimit(OrderEngine order)
    {
       
        bool cancelled = false;
        if (order.Side == SideTrade.Buy)
        {
            var sellOrders = _matchingCache.GetSellOrderBySymbol(order.Symbol).Result.Value;
            var orderToTrade = sellOrders.FirstOrDefault(kvp => kvp.Value.Price <= order.Price && 
                                                               kvp.Value.Quantity == order.Quantity);
            if (!orderToTrade.Equals(default(KeyValuePair<long, OrderEngine>)))
            {
                _tradeOrder.CreateTradeCapture(order, orderToTrade.Value);
                _tradeOrder.RemoveTradedOrdersAsync(order, orderToTrade.Value);
            }else
            {
                if (order.TimeInForce == TimeInForce.FOK)
                    cancelled = _tradeOrder.RemoveCancelledOrdersAsync(order).Result;
            }
        }
        else if (order.Side == SideTrade.Sell)
        {
            var buyOrders = _matchingCache.GetBuyOrderBySymbol(order.Symbol).Result.Value;
            var orderToTrade = buyOrders.FirstOrDefault(kvp => kvp.Value.Price >= order.Price && 
                                                               kvp.Value.Quantity == order.Quantity);
            if (!orderToTrade.Equals(default(KeyValuePair<long, OrderEngine>)))
            {
                _tradeOrder.CreateTradeCapture(orderToTrade.Value, order);
                _tradeOrder.RemoveTradedOrdersAsync(orderToTrade.Value, order);
            }
            else
            {
                if (order.TimeInForce == TimeInForce.FOK)
                    cancelled = _tradeOrder.RemoveCancelledOrdersAsync(order).Result;
            }
        }
    }

    protected override void MatchOrderMarket(OrderEngine order)
    {
        throw new NotImplementedException();
    }
}