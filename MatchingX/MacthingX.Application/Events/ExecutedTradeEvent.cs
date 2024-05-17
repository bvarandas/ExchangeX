﻿using MatchingX.Core;
using MediatR;
using SharedX.Core.Events;
using SharedX.Core.Matching.DropCopy;

namespace MacthingX.Application.Events;
public class ExecutedTradeEvent : Event
{
    public Dictionary<long, DropCopyReport> ExecutedTrades {  get; private set; }
    public DateTime Timestamp { get; private set; }
    public ExecutedTradeEvent(Dictionary<long, DropCopyReport> executedTrade)
    {
        ExecutedTrades = executedTrade;
        Timestamp = DateTime.Now;
    }
}