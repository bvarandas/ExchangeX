﻿using MacthingX.Application.Commands.Order;
using SharedX.Core.Matching.OrderEngine;
namespace MacthingX.Application.Commands.Match;
public class MatchingStopCommand : MatchingEngineCommand
{
    public MatchingStopCommand(OrderEngine order)
    {
        Timestamp = DateTime.Now;
        Order = order;
    }
    public override bool IsValid()
    {
        return true;
    }
}