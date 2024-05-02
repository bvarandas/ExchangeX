using MatchingX.Core.Filters;
using SharedX.Core.Matching;
namespace MatchingX.Core.Repositories;
public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetOrdersOnRestartAsync(OrderParams specParams, CancellationToken cancellation);
    Task<IEnumerable<Order>> GetOrdersAsync(OrderParams specParams, CancellationToken cancellation);
    Task<bool> CreateOrdersAsync(Order order, CancellationToken cancellation);
    Task<bool> UpdateOrderAsync(Order order, CancellationToken cancellation);
}