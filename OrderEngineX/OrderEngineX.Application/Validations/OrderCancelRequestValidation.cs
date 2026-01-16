using OrderEngineX.Application.Commands.Order;
namespace OrderEngineX.Application.Validations;
public class OrderCancelRequestValidation : OrderEngineValidation<OrderCancelCommand>
{
    public OrderCancelRequestValidation() : base()
    {
        ValidateOrderCancelRequest();
    }
}