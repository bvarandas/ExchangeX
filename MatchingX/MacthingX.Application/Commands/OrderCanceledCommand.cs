﻿using MediatR;
using SharedX.Core.Matching;
using SharedX.Core.Enums;
using SharedX.Core.Commands;

namespace MacthingX.Application.Events;
public class OrderCanceledCommand : Command
{
    public readonly Order Order;
    public DateTime Timestamp { get; private set; }
    public OrderCanceledCommand(Order order)
    {
        Timestamp = DateTime.Now;
        Order = order;
    }
}