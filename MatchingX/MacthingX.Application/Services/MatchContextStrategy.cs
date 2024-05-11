﻿using MatchingX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.Enums;
using SharedX.Core.Interfaces;
namespace MacthingX.Application.Services;
public class MatchContextStrategy
{
    private IMatch _match;
    private readonly IOrderRepository _orderRepository;
    private readonly IMatchingCache _matchingCache;

    public MatchContextStrategy() { }
    public MatchContextStrategy(IMatch match, 
        IOrderRepository orderRepository, 
        IMatchingCache matchingCache)
    {
        this._match = match;
        this._orderRepository = orderRepository;
        this._matchingCache = matchingCache;

        LoadOrdersOnRestart();
    }

    public void SetStrategy(IMatch match)
    {
        this._match = match;
    }

    private void LoadOrdersOnRestart()
    {
        var ordersDb = _orderRepository.GetOrdersOnRestartAsync(default(CancellationToken));

        var buyOrders = ordersDb.Result.Where(o => o.Side == SideTrade.Buy);
        var sellOrders = ordersDb.Result.Where(o => o.Side == SideTrade.Sell);

        foreach (var order in buyOrders)
            _matchingCache.UpsertBuyOrder(order);

        foreach (var order in sellOrders)
            _matchingCache.UpsertSellOrder(order);
    }
    public void ReceivedOrder(OrderEngine order)
    {
        this._match.ReceiveOrder(order);
    }
    public async Task<bool> MatchOrderAsync(OrderEngine order)
    {
        bool result = false;
        if (order.Side == SideTrade.Buy)
            result  = await this._match.MatchBuyOrderAsync(order);
        else if (order.Side == SideTrade.Sell)
            result = await this._match.MatchBuyOrderAsync(order);

        return result;
    }
}
