using SharedX.Core.Matching;

namespace MacthingX.Application.Interfaces;
public interface IMatchMarket
{
    void ReceiveOrder(Order order);
}
