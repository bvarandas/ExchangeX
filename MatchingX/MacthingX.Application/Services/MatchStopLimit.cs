﻿using MacthingX.Application.Commands.Match.OrderType;
using MacthingX.Application.Interfaces;
using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SharedX.Core.Bus;
using SharedX.Core.Enums;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;
using System.Collections.Concurrent;
namespace MacthingX.Application.Services;
public class MatchStopLimit :  IMatch
{
    protected readonly IOrderStopCache _orderStopCache;
    private readonly ConcurrentDictionary<long, OrderEngine> DicOrdersToCancel;
    protected readonly ITradeOrderService _tradeOrder;
    protected readonly IMediatorHandler Bus;

    public string Name => nameof(MatchStopLimit);

    public MatchStopLimit(ILogger<MatchStopLimit> logger, 
        IMediatorHandler bus,
        IOrderStopCache orderStopCache,
        ITradeOrderService tradeOrder)
    {
        DicOrdersToCancel = new ConcurrentDictionary<long, OrderEngine>();

        _orderStopCache = orderStopCache;
        _tradeOrder = tradeOrder;
        Bus = bus;
        _tradeOrder.PriceChanged += TradeOrder_PriceChanged;
    }

    private async void TradeOrder_PriceChanged(object sender, Events.OrderPriceEventArgs args)
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
                        await this.MatchOrderAsync(order);

                        break;
                    case (OrderType.Stop, SideTrade.Buy) when order.StopPrice <= price:
                        await _orderStopCache.DeleteOrderAsync(order.Symbol, order.OrderID);
                        _tradeOrder.AddOrder(order);
                        await this.MatchOrderAsync(order);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public  void ReceiveOrder(OrderEngine order)
    {
        switch (order.Execution)
        {
            case Execution.ToCancel:
                this.CancelOrder(order);
                break;
            case Execution.ToModify:
                this.ModifyOrder(order);
                break;
            case Execution.ToOpen:
                _tradeOrder.AddOrder(order);
                break;
        }
    }

    public   bool CancelOrder(OrderEngine orderToCancel)
    {
        bool cancelled = false;
        DicOrdersToCancel.TryAdd(orderToCancel.OrderID, orderToCancel);
        _tradeOrder.CancelOrder(orderToCancel);
        cancelled = _orderStopCache.DeleteOrderAsync(orderToCancel.Symbol, orderToCancel.OrderID).Result;
        return cancelled;
    }

    public  bool ModifyOrder(OrderEngine order)
    {
        return _tradeOrder.ModifyOrder(order).Result;
    }

    public  async Task<bool> MatchOrderAsync(OrderEngine order)
    {
        bool cancelled = false;
        var result = Bus.SendMatchCommand(new MatchingStopLimitCommand(order)).Result;

        if (result.Item1 is OrderStatus.Filled or OrderStatus.PartiallyFilled)
        {
            _tradeOrder.CreateReports(order, result.Item2);
            await _tradeOrder.RemoveTradedOrdersAsync(result.Item2);
        }
        else
        {
            if (order.TimeInForce == TimeInForce.FOK)
                cancelled = _tradeOrder.RemoveCancelledOrdersAsync(order).Result;
        }
        return true;
    }
}