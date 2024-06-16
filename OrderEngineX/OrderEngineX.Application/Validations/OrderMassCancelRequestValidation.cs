using OrderEngineX.Application.Commands.Order;
using OrderEngineX.Core.Interfaces;
using SharedX.Core.Interfaces;
namespace MacthingX.Application.Validations;
public class OrderMassCancelRequestValidation : OrderValidation<OrderCancelCommand>
{
    public OrderMassCancelRequestValidation(IBookOfferCache matchingCache, ISecurityEngineCache securityEngineCache) :base(matchingCache, securityEngineCache)
    {
        ValidateMassCancelOrder();
    }
}