using SharedX.Core.Matching.OrderEngine;
namespace OrderEngineX.Core.Interfaces;
public interface IPublisherOrderApp
{
    Task AddOrderToQueue(OrderEngine order);
}
