using MatchingX.Core.Interfaces;
using OrderEngineX.Application.Commands;

namespace OrderEngineX.Application.Validations;
public class OrderCancelReplaceRequestValidation : OrderEngineValidation<OrderCancelReplaceCommand>
{
    public OrderCancelReplaceRequestValidation(IBookOfferCache bookCache) : base(bookCache)
    {
        ValidateOrderCancelReplaceRequest();
    }
}