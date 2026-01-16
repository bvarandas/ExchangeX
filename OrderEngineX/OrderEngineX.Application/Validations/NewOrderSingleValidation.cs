using OrderEngineX.Application.Commands.Order;
namespace OrderEngineX.Application.Validations;
public class NewOrderSingleValidation : OrderEngineValidation<OrderOpenedCommand>
{
    public NewOrderSingleValidation() : base()
    {
        ValidateNewOrderSingle();
    }
}
