using FluentResults;
using SharedX.Core.Matching.OrderEngine;
namespace OrderEngineX.Core.Interfaces;
public interface IOrderEngineRepository
{
    Task<Result<bool>> UpsertOrdersAsync(OrderEngine order, CancellationToken cancellationToken);
}
