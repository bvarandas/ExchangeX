using DropCopyX.Core.Filters;
using SharedX.Core.Matching;
namespace DropCopyX.Core.Interfaces;
public interface IOrderFixRepository
{
    Task<IEnumerable<OrderEng>> GetOrdersAsync(OrderFixParams specParams, CancellationToken cancellation);
    Task<bool> CreateOrdersAsync(OrderEng order, CancellationToken cancellation);
    Task<bool> UpdateOrderAsync(OrderEng order, CancellationToken cancellation);
}
