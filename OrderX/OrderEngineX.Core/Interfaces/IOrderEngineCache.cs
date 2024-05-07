using SharedX.Core.Matching;
using SharedX.Core.Matching.OrderEngine;

namespace OrderEngineX.Core.Interfaces;
public interface IOrderEngineCache
{
    void AddOrder(OrderEngine order);
    bool TryDequeueOrder(out OrderEngine    order);
}