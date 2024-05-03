using SharedX.Core.Matching;
namespace SharedX.Core.Interfaces;
public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetOrdersOnRestartAsync(CancellationToken cancellation);
    Task<IEnumerable<Order>> GetOrdersByAccountIdAsync(int accountId, CancellationToken cancellation);
    Task<bool> CreateOrdersAsync(Order order, CancellationToken cancellation);
    Task<bool> UpdateOrderAsync(Order order, CancellationToken cancellation);
    Task<long> GetOrderIdAsync(CancellationToken cancellation);
}