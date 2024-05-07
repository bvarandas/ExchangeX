using OrderEntryX.Core.Filters;
using SharedX.Core.Matching.OrderEngine;
namespace OrderEntryX.Core.Interfaces;
public interface IOrderFixRepository
{
    Task<IEnumerable<OrderEngine>> GetOrdersAsync(OrderFixParams specParams, CancellationToken cancellation);
    Task<bool> CreateOrdersAsync(OrderEngine order, CancellationToken cancellation);
    Task<bool> UpdateOrderAsync(OrderEngine order, CancellationToken cancellation);
}
