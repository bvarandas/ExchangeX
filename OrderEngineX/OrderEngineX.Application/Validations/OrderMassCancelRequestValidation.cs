using MatchingX.Core.Interfaces;
using OrderEngineX.Application.Commands.Order;
using OrderEngineX.Core.Interfaces;
namespace MacthingX.Application.Validations;
public class OrderMassCancelRequestValidation : OrderValidation<OrderCancelCommand>
{
    public OrderMassCancelRequestValidation(IBookOfferCache bookCache, ISecurityEngineCache securityEngineCache) : base(bookCache, securityEngineCache)
    {
        ValidateMassCancelOrder();
    }
}