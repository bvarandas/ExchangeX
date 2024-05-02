using MarketDataX.Core.Entities;
using FluentResults;
namespace MarketDataX.Core.Repositories;

public interface ILoginRepository
{
    Task<Result<bool>> AddLogin(Login login, CancellationToken cancellationToken);
    Task<Result<bool>> RemoveLogin(Login login, CancellationToken cancellationToken);
    Task<Result<bool>> UpdateLogin(Login login, CancellationToken cancellationToken);
    Task<Result<bool>> ExecuteLogin(Login login, CancellationToken cancellationToken);
}
