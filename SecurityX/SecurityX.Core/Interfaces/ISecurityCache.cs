using FluentResults;
using SharedX.Core.Entities;

namespace SecurityX.Core.Interfaces;
public interface ISecurityCache
{
    Task<Result<SecurityEngine>> GetSecurityBySymbolAsync(string symbol);
    Task<Result<Dictionary<string, SecurityEngine>>> GetAllSecurityAsync();
    Task UpsertSecurityAsync(SecurityEngine security);
}
