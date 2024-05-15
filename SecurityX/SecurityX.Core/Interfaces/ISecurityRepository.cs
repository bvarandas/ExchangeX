using MongoDB.Driver;
using SharedX.Core.Entities;
namespace SecurityX.Core.Interfaces;
public interface ISecurityRepository
{
    Task<Dictionary<string, SecurityEngine>> GetAllSecurityiesAsync(CancellationToken cancellationToken);
}
