using OrderEngineX.Application.Commands;
namespace OrderEngineX.Application.Validations;
public class OrderCancelReplaceRequestValidation : OrderEngineValidation<OrderTradeModifyCommand>
{
    public OrderCancelReplaceRequestValidation()
    {
        ValidateOrderCancelReplaceRequest();
    }
}