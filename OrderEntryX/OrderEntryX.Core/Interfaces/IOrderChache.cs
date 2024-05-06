using SharedX.Core.Matching.OrderEngine;
namespace OrderEntryX.Core.Interfaces;
public interface IOrderEntryChache
{
    void AddOrderEntryAsync(OrderEngine order);
    bool TryDequeueOrderEntry(out OrderEngine order);
}
