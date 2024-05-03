using SharedX.Core.Matching;

namespace OrderEngineX.Core.Interfaces;
public interface IPublisherOrderApp
{
    Task AddOrderToQueue(Order order);
}
