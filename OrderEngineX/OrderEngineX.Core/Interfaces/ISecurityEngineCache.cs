using SharedX.Core.Entities;
namespace OrderEngineX.Core.Interfaces;
public interface ISecurityEngineCache
{
    bool TryGetSecurity(string symbol, out SecurityEngine security);
}