using MacthingX.Application.Events;
using MatchingX.Core.Interfaces;
namespace MacthingX.Application.Validations;
public class OrderCancelRequestValidation : OrderValidation<OrderCancelCommand>
{
    public OrderCancelRequestValidation(IMatchingCache matchingCache) : base(matchingCache)
    {
        ValidateOrderCancelRequest();
    }
}