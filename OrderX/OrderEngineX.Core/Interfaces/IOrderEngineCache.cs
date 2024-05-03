using SharedX.Core.Matching;
namespace OrderEngineX.Core.Interfaces;
public interface IOrderEngineCache
{
    void AddOrder(Order order);
    bool TryDequeueOrder(ref Order order);
}