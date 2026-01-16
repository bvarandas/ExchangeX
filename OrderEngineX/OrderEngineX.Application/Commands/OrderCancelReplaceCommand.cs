using OrderEngineX.Application.Validations;
using SharedX.Core.Matching.OrderEngine;
namespace OrderEngineX.Application.Commands;
public class OrderCancelReplaceCommand : OrderEngineCommand
{
    public OrderCancelReplaceCommand(OrderEngine order)
    {
        Timestamp = DateTime.Now;
        Order = order;
    }
    public override bool IsValid()
    {
        ValidationResult = new OrderCancelReplaceRequestValidation().Validate(this);
        return ValidationResult.IsValid;
    }
}