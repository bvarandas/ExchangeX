using MatchingX.Core.Interfaces;
using OrderEngineX.Application.Commands.Order;
namespace OrderEngineX.Application.Validations;
public class NewOrderSingleValidation : OrderEngineValidation<OrderOpenedCommand>
{
    public NewOrderSingleValidation(IBookOfferCache bookCache) : base(bookCache)
    {
        ValidateNewOrderSingle();
    }
}
