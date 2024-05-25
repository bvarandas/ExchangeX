using OrderEngineX.Application.Commands.Order;
using SharedX.Core.Interfaces;
namespace MacthingX.Application.Validations;
public class OrderMassCancelRequestValidation : OrderValidation<OrderCancelCommand>
{
    public OrderMassCancelRequestValidation(IMatchingCache matchingCache) :base(matchingCache)
    {
        ValidateMassCancelOrder();
    }
}