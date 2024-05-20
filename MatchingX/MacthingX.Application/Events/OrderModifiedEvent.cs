using SharedX.Core.Events;
using SharedX.Core.Matching.OrderEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacthingX.Application.Events;

public class OrderModifiedEvent : Event
{
    public readonly OrderEngine Order;
    public OrderModifiedEvent(OrderEngine order)
    {
        Order = order;
        Timestamp = DateTime.Now;
    }
}
