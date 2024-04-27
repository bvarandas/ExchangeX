using QuickFix;
namespace MatchingX.Infra.FixClientApp;
public interface ITradeClientApp: IApplication
{
    void SendMessage(Message m);
}
