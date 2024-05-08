using MacthingX.Application.Commands;
using MatchingX.Core.Interfaces;
namespace MacthingX.Application.Validations;
public class OrderCancelReplaceRequestValidation : OrderValidation<OrderCancelReplaceCommand>
{
    public OrderCancelReplaceRequestValidation(IMatchingCache matchingCache) : base(matchingCache)
    {
        ValidateCancelReplaceOrder();
    }
}