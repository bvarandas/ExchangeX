using OrderEngineX.Application.Validations;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;
namespace OrderEngineX.Application.Commands;
public class OrderCancelReplaceCommand : OrderEngineCommand
{
    private readonly IBookOfferCache _cache;
    public OrderCancelReplaceCommand(OrderEngine order, IBookOfferCache cache)
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