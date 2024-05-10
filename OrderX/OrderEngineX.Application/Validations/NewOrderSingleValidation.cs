using MarketDataX.Application.Commands;
namespace OrderEngineX.Application.Validations;
public class NewOrderSingleValidation : OrderEngineValidation<OrderTradeNewCommand>
{
    public NewOrderSingleValidation()
    {
        ValidateNewOrderSingle();
    }
}
