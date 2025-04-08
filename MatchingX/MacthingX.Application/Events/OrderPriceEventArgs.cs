using MatchingX.Core.Entities;

namespace MacthingX.Application.Events;

public class OrderPriceEventArgs : EventArgs
{
    public MatchOrder Order { get; private set; }
    public OrderPriceEventArgs(MatchOrder order)
    {
        Order = order;
    }
}
