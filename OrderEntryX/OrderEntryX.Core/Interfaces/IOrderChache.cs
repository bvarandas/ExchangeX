using SharedX.Core.Matching;

namespace OrderEntryX.Core.Interfaces;
public interface IOrderEntryChache
{
    void AddOrderEntryAsync(Order order);
    bool TryDequeueOrderEntry(out Order order);
}
