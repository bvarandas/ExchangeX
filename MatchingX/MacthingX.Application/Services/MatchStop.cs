using MacthingX.Application.Events;
using MacthingX.Application.Interfaces;
using MassTransit;
using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SharedX.Core.Bus;
using SharedX.Core.Enums;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;
using System.Collections.Concurrent;

namespace MacthingX.Application.Services;
public class MatchStop : IMatchStop, IMatch
{
    private readonly ConcurrentDictionary<long, OrderEngine> DicOrdersToCancel;
    protected readonly ITradeOrderService _tradeOrder;
    protected readonly IMatchingCache _matchingCache;
    protected readonly IOrderStopCache _orderStopCache;

    public MatchStop(ILogger<MatchStop> logger, IMediatorHandler bus, 
        IMatchingCache matchingCache,
        IOrderStopCache orderStopCache,
        ITradeOrderService tradeOrder) 
    {
        DicOrdersToCancel = new ConcurrentDictionary<long, OrderEngine>();

        _tradeOrder = tradeOrder;
        _matchingCache = matchingCache;
        _orderStopCache = orderStopCache;
        _tradeOrder.PriceChanged += TradeOrder_PriceChanged;
    }

    private async void TradeOrder_PriceChanged(object sender, OrderPriceEventArgs args)
    {
        var order = args.Order;
        decimal price = order.Price;

        var ordersStop = await _orderStopCache.GetOrderBySymbolAsync(order.Symbol);
        if (ordersStop.IsSuccess)
        {
            foreach (var ordersStopPrice in ordersStop.Value)
            {
                if (DicOrdersToCancel.TryGetValue(order.OrderID, out OrderEngine orderFound))
                    DicOrdersToCancel.Remove(order.OrderID, out OrderEngine orderRemoved);

                switch ((order.OrderType, order.Side))
                {
                    case (OrderType.Stop, SideTrade.Sell) when order.StopPrice >= price:
                        await _orderStopCache.DeleteOrderAsync(order.Symbol, order.OrderID);
                        _tradeOrder.AddOrder(order);
                        await this.MatchSellOrderAsync(order);

                        break;
                    case (OrderType.Stop, SideTrade.Buy) when order.StopPrice <= price:
                        await _orderStopCache.DeleteOrderAsync(order.Symbol, order.OrderID);
                        _tradeOrder.AddOrder(order);
                        await this.MatchBuyOrderAsync(order);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public bool AddOrder(OrderEngine order)
    {
        switch (order.Execution)
        {
            case Execution.ToCancel when order.Side == SideTrade.Sell:
                this.CancelOrder(order);
                break;
            case Execution.ToCancel when order.Side == SideTrade.Buy:
                this.CancelOrder(order);
                break;
            case Execution.ToCancelReplace when order.Side == SideTrade.Sell:
                this.CancelOrder(order);
                this.ReplaceOrder(order);
                break;
            case Execution.ToCancelReplace when order.Side == SideTrade.Buy:
                this.CancelOrder(order);
                this.ReplaceOrder(order);
                break;
            case Execution.ToOpen:
                _orderStopCache.UpsertOrderAsync(order);
                break;
        }
        return true;
    }

    public void ReceiveOrder(OrderEngine order)
    {
        this.AddOrder(order);
    }
    public bool ReplaceOrder(OrderEngine order)
    {
        return _tradeOrder.ReplaceOrder(order);
    }

    public bool CancelOrder(OrderEngine orderToCancel)
    {
        bool cancelled = false;
        DicOrdersToCancel.TryAdd(orderToCancel.OrderID, orderToCancel);
        _tradeOrder.CancelOrder(orderToCancel);
        cancelled = _orderStopCache.DeleteOrderAsync(orderToCancel.Symbol, orderToCancel.OrderID).Result;

        return cancelled;
    }

    public async Task<bool> MatchBuyOrderAsync(OrderEngine order)
    {
        bool cancelled = false;
        var sellOrders = _matchingCache.GetSellOrderBySymbol(order.Symbol).Result.Value;
        var orderToTrade = sellOrders
            .OrderBy(p=>p.Value.Price)
            .FirstOrDefault(sell => sell.Value.Quantity == order.Quantity);

        if (!orderToTrade.Equals(default(KeyValuePair<long, OrderEngine>)))
        {
            _tradeOrder.CreateTradeCapture(order, orderToTrade.Value);
            await _tradeOrder.RemoveTradedOrdersAsync(order, orderToTrade.Value);
        }
        else
        {
            if (order.TimeInForce == TimeInForce.FOK)
                cancelled = _tradeOrder.RemoveCancelledOrdersAsync(order).Result;
        }
        return true;
    }

    public async Task<bool> MatchSellOrderAsync(OrderEngine order)
    {
        bool cancelled = false;
        var buyOrders = _matchingCache.GetBuyOrderBySymbol(order.Symbol).Result.Value;
        var orderToTrade = buyOrders
            .OrderByDescending(p=>p.Value.Price)
            .FirstOrDefault(kvp => kvp.Value.Quantity == order.Quantity);

        if (!orderToTrade.Equals(default(KeyValuePair<long, OrderEngine>)))
        {
            _tradeOrder.CreateTradeCapture(orderToTrade.Value, order);
            await _tradeOrder.RemoveTradedOrdersAsync(orderToTrade.Value, order);
        }
        else
        {
            if (order.TimeInForce == TimeInForce.FOK)
                cancelled = _tradeOrder.RemoveCancelledOrdersAsync(order).Result;
        }
        return true;
    }

    public bool ModifyOrder(OrderEngine order)
    {
        throw new NotImplementedException();
    }
}