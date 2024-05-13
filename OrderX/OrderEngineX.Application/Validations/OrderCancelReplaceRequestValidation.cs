using OrderEngineX.Application.Commands;
namespace OrderEngineX.Application.Validations;
public class OrderCancelReplaceRequestValidation : OrderEngineValidation<OrderTradeCancelReplaceCommand>
{
    public OrderCancelReplaceRequestValidation()
    {
        ValidateOrderCancelReplaceRequest();
    }
}