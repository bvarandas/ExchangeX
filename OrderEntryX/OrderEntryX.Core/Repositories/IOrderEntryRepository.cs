using FluentResults;
using OrderEntryX.Core.Entities;

namespace OrderEntryX.Core.Repositories;
public interface IOrderEntryRepository
{
    Task<Result> CreateOrdersAsync(OrderEntry order, CancellationToken cancellationToken);
}
