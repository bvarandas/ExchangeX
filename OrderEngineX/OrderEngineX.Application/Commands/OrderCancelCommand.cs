using OrderEngineX.Application.Validations;
using SharedX.Core.Matching.OrderEngine;
namespace OrderEngineX.Application.Commands.Order;
public class OrderCancelCommand : OrderEngineCommand
{
    public OrderCancelCommand(OrderEngine order)
    {
        Timestamp = DateTime.Now;
        Order = order;
    }
    public override bool IsValid()
    {
        ValidationResult = new OrderCancelRequestValidation().Validate(this);
        return ValidationResult.IsValid;
    }
}