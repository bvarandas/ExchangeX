﻿using SharedX.Core.Events;
using SharedX.Core.Matching.OrderEngine;
namespace OrderEngineX.Application.Events;
public class OrderTradeNewEvent: Event
{
    public readonly OrderEngine Order;
    public DateTime Timestamp { get; private set; }
    public OrderTradeNewEvent(OrderEngine order )
    {
        Order = order;
        Timestamp = DateTime.Now;
    }
}