using MacthingX.Application.Events;
using MatchingX.Core.Interfaces;
namespace MacthingX.Application.Validations;
public class NewOrderSingleValidation : OrderValidation<OrderOpenedCommand>
{
    public NewOrderSingleValidation(IMatchingCache matchingCache):base(matchingCache)
    {
        ValidateNewOrderSingle();
    }
}
