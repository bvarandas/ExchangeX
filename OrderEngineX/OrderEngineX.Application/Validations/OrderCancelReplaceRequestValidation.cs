using OrderEngineX.Application.Commands;
using SharedX.Core.Interfaces;

namespace OrderEngineX.Application.Validations;
public class OrderCancelReplaceRequestValidation : OrderEngineValidation<OrderCancelReplaceCommand>
{
    public OrderCancelReplaceRequestValidation(IBookOfferCache  matchingCache) : base(matchingCache)
    {
        ValidateOrderCancelReplaceRequest();
    }
}