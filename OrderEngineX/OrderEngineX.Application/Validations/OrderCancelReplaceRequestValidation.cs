using OrderEngineX.Application.Commands;

namespace OrderEngineX.Application.Validations;
public class OrderCancelReplaceRequestValidation : OrderEngineValidation<OrderCancelReplaceCommand>
{
    public OrderCancelReplaceRequestValidation() : base()
    {
        ValidateOrderCancelReplaceRequest();
    }
}