using OrderEngineX.Application.Commands.Order;
using SharedX.Core.Interfaces;
namespace OrderEngineX.Application.Validations;
public class NewOrderSingleValidation : OrderEngineValidation<OrderOpenedCommand>
{
    public NewOrderSingleValidation(IMatchingCache matchingCache) : base(matchingCache)
    {
        ValidateNewOrderSingle();
    }
}
