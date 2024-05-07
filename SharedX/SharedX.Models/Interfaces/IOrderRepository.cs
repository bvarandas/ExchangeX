using SharedX.Core.Matching;
using SharedX.Core.Matching.OrderEngine;

namespace SharedX.Core.Interfaces;
public interface IOrderRepository
{
    Task<IEnumerable<OrderEngine>> GetOrdersOnRestartAsync(CancellationToken cancellation);
    Task<IEnumerable<OrderEngine>> GetOrdersByAccountIdAsync(int accountId, CancellationToken cancellation);
    Task<bool> CreateOrdersAsync(OrderEngine order, CancellationToken cancellation);
    Task<bool> UpdateOrderAsync(OrderEngine order, CancellationToken cancellation);
    Task<long> GetOrderIdAsync(CancellationToken cancellation);
}