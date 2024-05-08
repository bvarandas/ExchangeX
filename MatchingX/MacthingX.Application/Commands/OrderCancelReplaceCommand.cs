using MacthingX.Application.Validations;
using MatchingX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;
namespace MacthingX.Application.Commands;
public class OrderCancelReplaceCommand : OrderEngineCommand
{
    private readonly IMatchingCache _cache;

    public OrderCancelReplaceCommand(OrderEngine order, IMatchingCache cache)
    {
        Timestamp = DateTime.Now;
        Order = order;
        _cache = cache;
    }

    public override bool IsValid()
    {
        ValidationResult = new OrderCancelReplaceRequestValidation(_cache).Validate(this);
        return ValidationResult.IsValid;
    }
}