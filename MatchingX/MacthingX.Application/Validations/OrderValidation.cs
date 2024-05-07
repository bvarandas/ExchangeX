using FluentValidation;
namespace MacthingX.Application.Validations;
public class OrderValidation<T> : AbstractValidator<T> where T: class
{
    protected void ValidateNewOrderSingle()
    {

    }

    protected void ValidateOrderCancelRequest()
    {

    }

    protected void ValidateCancelReplaceOrder()
    {

    }

    protected void ValidateMassCancelReplaceOrder()
    {

    }
}