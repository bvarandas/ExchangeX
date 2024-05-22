using OrderEngineX.Application.Commands;
using SharedX.Core.Interfaces;

namespace OrderEngineX.Application.Validations;
public class OrderCancelReplaceRequestValidation : OrderEngineValidation<OrderCancelReplaceCommand>
{
    public OrderCancelReplaceRequestValidation(IMatchingCache  matchingCache) : base(matchingCache)
    {
        ValidateOrderCancelReplaceRequest();
    }
}