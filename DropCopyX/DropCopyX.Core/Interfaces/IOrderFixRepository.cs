using DropCopyX.Core.Filters;
using SharedX.Core.Matching.OrderEngine;
namespace DropCopyX.Core.Interfaces;
public interface IOrderFixRepository
{
    Task<IEnumerable<OrderEngine>> GetOrdersAsync(OrderFixParams specParams, CancellationToken cancellation);
    Task<bool> CreateOrdersAsync(OrderEngine order, CancellationToken cancellation);
    Task<bool> UpdateOrderAsync(OrderEngine order, CancellationToken cancellation);
}
