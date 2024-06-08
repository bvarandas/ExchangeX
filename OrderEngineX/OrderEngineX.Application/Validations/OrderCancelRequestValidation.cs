using OrderEngineX.Application.Commands.Order;
using SharedX.Core.Interfaces;
namespace OrderEngineX.Application.Validations;
public class OrderCancelRequestValidation: OrderEngineValidation<OrderCancelCommand>
{
    public OrderCancelRequestValidation(IBookOfferCache matchingCache) : base(matchingCache)
    {
        ValidateOrderCancelRequest();
    }
}