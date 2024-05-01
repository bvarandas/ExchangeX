﻿using SharedX.Core.Enums;
using SharedX.Core.Matching;
using SharedX.Core.Proto;
using System.Collections.Concurrent;
namespace MacthingX.FixApp.Services;
public abstract class MatchLastPrice
{
    protected readonly ConcurrentDictionary<string, decimal> _lastPrice;

    public MatchLastPrice()
    {
        _lastPrice= new ConcurrentDictionary<string, decimal>();
    }

    protected void AddUpdatePrice(Order order)
    {
        if (order.OrderStatus.Equals(OrderStatus.Rejected) ||
            order.OrderStatus.Equals(OrderStatus.Cancelled))
            return;

            if (_lastPrice.TryGetValue(order.Symbol, out decimal price))
        {
            _lastPrice[order.Symbol] = price;
        }else
            _lastPrice.TryAdd(order.Symbol, order.Price);
    }
}