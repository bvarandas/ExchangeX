using OrderEngineX.Application.Validations;
using SharedX.Core.Matching.OrderEngine;
namespace OrderEngineX.Application.Commands;
public class OrderTradeCancelReplaceCommand : OrderEngineCommand
{
    public OrderTradeCancelReplaceCommand(OrderEngine order)
    {   
        Order = order;
        Timestamp = DateTime.Now;
    }
    public override bool IsValid()
    {
        ValidationResult = new OrderCancelReplaceRequestValidation().Validate(this);
        return ValidationResult.IsValid;
    }
}