using DropCopyX.Core.Filters;
using SharedX.Core.Matching;
namespace DropCopyX.Core.Interfaces;
public interface IOrderFixRepository
{
    Task<IEnumerable<Order>> GetOrdersAsync(OrderFixParams specParams, CancellationToken cancellation);
    Task<bool> CreateOrdersAsync(Order order, CancellationToken cancellation);
    Task<bool> UpdateOrderAsync(Order order, CancellationToken cancellation);
}
