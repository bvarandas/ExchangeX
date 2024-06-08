using OrderEngineX.Application.Commands.Order;
using SharedX.Core.Interfaces;
namespace MacthingX.Application.Validations;
public class OrderMassCancelRequestValidation : OrderValidation<OrderCancelCommand>
{
    public OrderMassCancelRequestValidation(IBookOfferCache matchingCache) :base(matchingCache)
    {
        ValidateMassCancelOrder();
    }
}