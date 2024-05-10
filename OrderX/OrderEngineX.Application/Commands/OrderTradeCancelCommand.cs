using OrderEngineX.Application.Commands;
using OrderEngineX.Application.Validations;
using SharedX.Core.Matching.OrderEngine;
namespace MarketDataX.Application.Commands;
public class OrderTradeCancelCommand : OrderEngineCommand
{
    public OrderTradeCancelCommand(OrderEngine order)
    {
        Order = order;
        Timestamp = DateTime.Now;
    }
    public override bool IsValid()
    {
        ValidationResult = new OrderCancelRequestValidation().Validate(this);
        return ValidationResult.IsValid;
    }
}