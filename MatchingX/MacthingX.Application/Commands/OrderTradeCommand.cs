﻿using MatchingX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;
namespace MacthingX.Application.Commands;
public class OrderTradeCommand : OrderEngineCommand
{
    private readonly IMatchingCache _cache;
    public OrderTradeCommand(OrderEngine order, IMatchingCache cache)
    {
        Timestamp = DateTime.Now;
        Order = order;
        _cache = cache;
    }
    public override bool IsValid()
    {
        return true;
    }
}
