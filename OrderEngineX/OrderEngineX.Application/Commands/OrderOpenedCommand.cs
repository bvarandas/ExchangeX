using OrderEngineX.Application.Validations;
using SharedX.Core.Matching.OrderEngine;
namespace OrderEngineX.Application.Commands.Order;
public class OrderOpenedCommand : OrderEngineCommand
{
    public OrderOpenedCommand(OrderEngine order)
    {
        Timestamp = DateTime.Now;
        Order = order;

    }
    public override bool IsValid()
    {
        ValidationResult = new NewOrderSingleValidation().Validate(this);
        return ValidationResult.IsValid;
    }
}