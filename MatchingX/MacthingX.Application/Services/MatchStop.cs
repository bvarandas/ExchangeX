using MacthingX.Application.Interfaces;
using MassTransit;
using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SharedX.Core.Bus;
using SharedX.Core.Enums;
using SharedX.Core.Matching.OrderEngine;
using System.Collections.Concurrent;
namespace MacthingX.Application.Services;
public class MatchStop : MatchBase, IMatchStop
{
    private readonly Thread ThreadOrdersStopPrice;
    private readonly ConcurrentQueue<OrderEngine> QueueOrderStopPrice;
    private readonly IMarketDataCache _cacheMarketData;
    private readonly ConcurrentDictionary<long, OrderEngine> DicOrdersToCancel;
    public MatchStop(ILogger<MatchBase> logger, IMediatorHandler bus, 
        IMatchingCache matchingCache, 
        IMarketDataCache marketDataCache,
        ITradeOrderService tradeOrder) 
        : base(logger, bus, matchingCache, marketDataCache, tradeOrder)
    {
        QueueOrderStopPrice = new ConcurrentQueue<OrderEngine>();
        
        ThreadOrdersStopPrice = new Thread(new ThreadStart(OrderStopVerifyReachPrice));
        ThreadOrdersStopPrice.Name = nameof(OrderStopVerifyReachPrice);
        ThreadOrdersStopPrice.Start();

        DicOrdersToCancel = new ConcurrentDictionary<long, OrderEngine>();
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
                if (DicOrdersToCancel.TryGetValue(order.OrderID, out OrderEngine orderFound))
                {
                    DicOrdersToCancel.Remove(order.OrderID, out OrderEngine orderRemoved);
                    continue;
                }

                decimal price = _cacheMarketData.GetPrice(symbol: order.Symbol).Result;

                if (!price.Equals(0))
                {
                    QueueOrderStopPrice.Enqueue(order);
                    Thread.Sleep(500);
                    continue;
                }

                switch ((order.OrderType, order.Side))
                {
                    case (OrderType.Stop, SideTrade.Sell) when order.StopPrice >= price:
                    case (OrderType.Stop, SideTrade.Buy) when order.StopPrice <= price:
                        // Adicionar no book e mandar ordem market
                        _tradeOrder.AddOrder(order);
                        this.MatchOrderMarket(order);
                        break;
                    default:
                        QueueOrderStopPrice.Enqueue(order);
                        
                        break;
                }
            }
            Thread.Sleep(500);
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
        DicOrdersToCancel.TryAdd(orderToCancel.OrderID, orderToCancel);
        _tradeOrder.CancelOrder(orderToCancel);
    }

    protected override void ReplaceOrder(OrderEngine order)
    {
        _tradeOrder.ReplaceOrder(order);
    }

    protected override void MatchOrderMarket(OrderEngine order)
    {
        bool cancelled = false;
        if (order.Side == SideTrade.Buy)
        {
            var sellOrders = _matchingCache.GetSellOrderBySymbol(order.Symbol).Result.Value;
            var orderToTrade = sellOrders.FirstOrDefault(sell=>sell.Value.Quantity == order.Quantity);

            if (!orderToTrade.Equals(default(KeyValuePair<long, OrderEngine>)))
            {
                _tradeOrder.CreateTradeCapture(order, orderToTrade.Value);
                _tradeOrder.RemoveTradedOrdersAsync(order, orderToTrade.Value);
            }else
            {
                if (order.TimeInForce == TimeInForce.FOK)
                    cancelled = _tradeOrder.RemoveCancelledOrdersAsync( order).Result;
            }
        }
        else if (order.Side == SideTrade.Sell)
        {
            var buyOrders = _matchingCache.GetBuyOrderBySymbol(order.Symbol).Result.Value;
            var orderToTrade = buyOrders.FirstOrDefault(kvp => kvp.Value.Quantity == order.Quantity);

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

    protected override void MatchOrderLimit(OrderEngine order)
    {
        throw new NotImplementedException();
    }

    
}
