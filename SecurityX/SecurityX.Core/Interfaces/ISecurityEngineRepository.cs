using FluentResults;
using SharedX.Core.Entities;
namespace SecurityX.Core.Interfaces;
public interface ISecurityEngineRepository
{
    Task<Result<Dictionary<string, SecurityEngine>>> GetAllSecurityiesAsync(CancellationToken cancellationToken);
    Task<Result<bool>> UpsertSecurityAsync(SecurityEngine security, CancellationToken cancellationToken);
    Task<Result<bool>> RemoveSecurityAsync(SecurityEngine security, CancellationToken cancellationToken);
}