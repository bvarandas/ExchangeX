using MarketDataX.Application.Commands;

namespace OrderEngineX.Application.Validations;
public class OrderCancelRequestValidation: OrderEngineValidation<OrderTradeCancelCommand>
{
    public OrderCancelRequestValidation()
    {
        ValidateOrderCancelRequest();
    }
}
