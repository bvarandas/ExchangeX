using SharedX.Core.Matching;
namespace OrderEngineX.Core.Interfaces;
public interface IOrderEngineCache
{
    void AddOrder(OrderEng order);
    bool TryDequeueOrder(out OrderEng order);
}