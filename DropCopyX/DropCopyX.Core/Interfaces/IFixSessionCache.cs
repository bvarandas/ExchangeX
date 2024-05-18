using QuickFix;
namespace DropCopyX.Core.Interfaces;
public interface IFixSessionCache
{
    void AddSessionAsync(QuickFix.FIX44.Message request, SessionID sessionID);
    Task<bool> RemoveSessionAsync(QuickFix.FIX44.Message request, SessionID sessionID);
}
