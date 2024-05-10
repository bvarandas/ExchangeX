using OrderEngineX.Application.Commands;
using OrderEngineX.Application.Validations;
using SharedX.Core.Matching.OrderEngine;
namespace MarketDataX.Application.Commands;
public class OrderTradeNewCommand : OrderEngineCommand
{
    public OrderTradeNewCommand(OrderEngine Order)
    {
        this.Order = Order;
        Timestamp = DateTime.Now;
    }

    public override bool IsValid()
    {
        ValidationResult = new NewOrderSingleValidation().Validate(this);
        return ValidationResult.IsValid;
    }
}