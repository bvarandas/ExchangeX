using SharedX.Core.Matching;
namespace MatchingX.Core.Interfaces;
public interface IMatchingReceiver
{
    void ReceiveOrder(Order order);
}