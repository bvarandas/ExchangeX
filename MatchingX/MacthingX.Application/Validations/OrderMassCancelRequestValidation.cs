using MacthingX.Application.Commands.Order;
using MatchingX.Core.Interfaces;
namespace MacthingX.Application.Validations;
public class OrderMassCancelRequestValidation : OrderValidation<OrderCancelCommand>
{
    public OrderMassCancelRequestValidation(IMatchingCache matchingCache) :base(matchingCache)
    {
        ValidateMassCancelOrder();
    }
}