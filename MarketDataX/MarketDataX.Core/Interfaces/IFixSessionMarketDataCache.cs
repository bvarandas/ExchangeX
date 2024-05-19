using QuickFix;
namespace MarketDataX.Core.Interfaces;
public interface IFixSessionMarketDataCache
{
    void AddSessionAsync(QuickFix.FIX44.Message request, SessionID sessionID);
    Task<bool> RemoveSessionAsync(QuickFix.FIX44.Message request, SessionID sessionID);
}
