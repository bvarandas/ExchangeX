using MacthingX.Application.Validations;
using MatchingX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;
namespace MacthingX.Application.Commands.Order;
public class OrderCancelCommand : OrderEngineCommand
{
    private readonly IMatchingCache _cache;
    public OrderCancelCommand(OrderEngine order, IMatchingCache cache)
    {
        Timestamp = DateTime.Now;
        Order = order;
        _cache = cache;
    }

    public override bool IsValid()
    {
        ValidationResult = new OrderCancelRequestValidation(_cache).Validate(this);
        return ValidationResult.IsValid;
    }
}