using MatchingX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacthingX.Application.Commands;

public class OrderPartiallyFilledCommand : OrderEngineCommand
{
    private readonly IMatchingCache _cache;
    public OrderPartiallyFilledCommand(OrderEngine order, IMatchingCache cache)
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
