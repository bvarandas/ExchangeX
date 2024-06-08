using OrderEngineX.Application.Commands.Order;
using SharedX.Core.Interfaces;
namespace OrderEngineX.Application.Validations;
public class NewOrderSingleValidation : OrderEngineValidation<OrderOpenedCommand>
{
    public NewOrderSingleValidation(IBookOfferCache matchingCache) : base(matchingCache)
    {
        ValidateNewOrderSingle();
    }
}
