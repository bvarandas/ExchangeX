using FluentResults;
using SharedX.Core.Entities;

namespace SharedX.Core.Repositories;

public interface ILoginRepository
{
    Task<Result<bool>> AddLogin(Login login, CancellationToken cancellationToken);
    Task<Result<bool>> RemoveLogin(Login login, CancellationToken cancellationToken);
    Task<Result<bool>> UpdateLogin(Login login, CancellationToken cancellationToken);
    Task<Result<bool>> ExecuteLogin(Login login, CancellationToken cancellationToken);
}
