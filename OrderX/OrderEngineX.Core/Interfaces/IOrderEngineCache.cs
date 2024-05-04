using SharedX.Core.Matching;
namespace OrderEngineX.Core.Interfaces;
public interface IOrderEngineCache
{
    void AddOrder(Order order);
    bool TryDequeueOrder(out Order order);
}