using MacthingX.Application.Validations;
using MatchingX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;
namespace MacthingX.Application.Commands.Order;
public class OrderOpenedCommand : OrderEngineCommand
{
    private readonly IMatchingCache _cache;
    public OrderOpenedCommand(OrderEngine order, IMatchingCache cache)
    {
        Timestamp = DateTime.Now;
        Order = order;
        _cache = cache;
    }
    public override bool IsValid()
    {
        ValidationResult = new NewOrderSingleValidation(_cache).Validate(this);
        return ValidationResult.IsValid;
    }
}