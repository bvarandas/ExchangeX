using FluentValidation;
using MacthingX.Application.Commands;
using MatchingX.Core.Interfaces;
using SharedX.Core.Enums;
using SharedX.Core.Matching.OrderEngine;
namespace MacthingX.Application.Validations;
public abstract class OrderValidation<T> : 
    AbstractValidator<T> where T: 
    OrderEngineCommand
{
    private readonly IMatchingCache _matchingCache;
    public OrderValidation(IMatchingCache matchingCache)
    {
        _matchingCache = matchingCache;
    }
    protected void ValidateNewOrderSingle()
    {
        /*
         *  0 = Other
            1 = Unknown ID
            2 = Unknown Security
            3 = Unknown Message Type
            4 = Application not available
            5 = Conditionally required field missing
            6 = Not Authorized
            7 = DeliverTo firm not available at this time
            18 = Invalid price increment
         */
        ValidateSymbol();
        ValidateNewOrderSingleOrderId();
        ValidateAccountId();
        ValidateParticipatorId();
        ValidateStopPx();
        ValidatePriceLimit();
        ValidatePriceMarket();
        ValidateTimeInForce();
    }

    protected void ValidateOrderCancelRequest()
    {
        /*
         0 = Too late to cancel
         1 = Unknown order
         2 = Broker / Exchange Option
         3 = Order already in Pending Cancel or Pending Replace status
         4 = Unable to process Order Mass Cancel Request <q>
         5 = OrigOrdModTime <586> did not match last TransactTime <60> of order
         6 = Duplicate ClOrdID <11> received
         */
        ValidateOrderToLateToCancel();
        ValidateOrderCancelOrderId();
        ValidateOrderExits();
    }
    protected void ValidateCancelReplaceOrder()
    {
        /*
         *  0 = Other
            1 = Unknown ID
            2 = Unknown Security
            3 = Unknown Message Type
            4 = Application not available
            5 = Conditionally required field missing
            6 = Not Authorized
            7 = DeliverTo firm not available at this time
            18 = Invalid price increment
         */
        ValidateClOrdID();
        ValidateSymbol();

        ValidateNewOrderSingleOrderId();
        ValidateAccountId();
        ValidateParticipatorId();
        ValidateStopPx();
        ValidatePriceLimit();
        ValidatePriceMarket();
        ValidateTimeInForce();

        /*
         0 = Too late to cancel
         1 = Unknown order
         2 = Broker / Exchange Option
         3 = Order already in Pending Cancel or Pending Replace status
         4 = Unable to process Order Mass Cancel Request <q>
         5 = OrigOrdModTime <586> did not match last TransactTime <60> of order
         6 = Duplicate ClOrdID <11> received
         */
        ValidateOrderToLateToCancel();
        ValidateOrderCancelOrderId();
        ValidateOrderExits();
    }
    protected void ValidateMassCancelOrder()
    {
        /*
         *  0 = Other
            1 = Unknown ID
            2 = Unknown Security
            3 = Unknown Message Type
            4 = Application not available
            5 = Conditionally required field missing
            6 = Not Authorized
            7 = DeliverTo firm not available at this time
            18 = Invalid price increment
         */
        ValidateNewOrderSingleOrderId();
        ValidateAccountId();
        ValidateParticipatorId();
        ValidateStopPx();
        ValidatePriceLimit();
        ValidatePriceMarket();
        ValidateTimeInForce();
        /*
         0 = Too late to cancel
         1 = Unknown order
         2 = Broker / Exchange Option
         3 = Order already in Pending Cancel or Pending Replace status
         4 = Unable to process Order Mass Cancel Request <q>
         5 = OrigOrdModTime <586> did not match last TransactTime <60> of order
         6 = Duplicate ClOrdID <11> received
         */
        ValidateOrderToLateToCancel();
        ValidateOrderCancelOrderId();
        ValidateOrderExits();
    }

    #region Private
    private void ValidateSymbol()
    {
        RuleFor(o => o.Order.Symbol)
            .NotEqual(string.Empty)
            .WithMessage("2-Unknown Security");
    }
    private void ValidateClOrdID()
    {
        RuleFor(o => o.Order.ClOrdID)
            .NotEqual(0)
            .WithMessage("5-Invalid ClOrderID order");
    }
    private void ValidateNewOrderSingleOrderId()
    {
        RuleFor(o => o.Order.OrderID)
            .NotEqual(0)
            .WithMessage("5-Invalid OrderID order");
    }
    private void ValidateAccountId()
    {
        RuleFor(o => o.Order.AccountId)
            .NotEqual(0)
            .WithMessage("5-Invalid accountId order");
    }
    private void ValidateParticipatorId()
    {
        RuleFor(o => o.Order.ParticipatorId)
            .NotEqual(0)
            .WithMessage("5-Invalid ParticipatorId order");
    }
    private void ValidatePriceLimit()
    {
        RuleFor(o => o.Order.Price)
            .Must(p=>p>0)
            .When(o=>o.Order.OrderType == OrderType.Limit || o.Order.OrderType== OrderType.StopLimit)
            .WithMessage("5-Invalid Price for limit or stopLimit order");
    }
    private void ValidatePriceMarket()
    {
        RuleFor(o => o.Order.Price)
            .Must(p => p == 0)
            .When(o => o.Order.OrderType == OrderType.Market || o.Order.OrderType == OrderType.Stop)
            .WithMessage("5-Invalid Price for market or stop order, must be 0 (zero)!");
    }
    private void ValidateStopPx()
    {
        RuleFor(o => o.Order.StopPrice)
            .Must(p => p > 0)
            .When(p => p.Order.OrderType == OrderType.Stop || p.Order.OrderType == OrderType.StopLimit)
            .WithMessage("5-Invalid StopPx to order stop or stoplimit");
    }
    private void ValidateTimeInForce()
    {
        RuleFor(o => o.Order.TimeInForce)
            .Must(p => p== TimeInForce.FOK)
            .When(p => p.Order.OrderType == OrderType.Market|| p.Order.OrderType == OrderType.Stop)
            .WithMessage("5-Invalid TimeInForce to order market or stop");
    }
    private void ValidateOrderCancelOrderId()
    {
        RuleFor(o => o.Order.OrderID)
            .NotEqual(0)
            .WithMessage("1-Invalid OrderID order");
    }
    private void ValidateOrderToLateToCancel()
    {
        RuleFor(o => o.Order)
            .Must(IsValidOrderToCancel)
            .WithMessage("0-To Late to cancel");
    }
    
    private bool IsOrderExists(OrderEngine order)
    {
        var orderFound = _matchingCache.GetBuyOrderByIdandSymbolAsync(order.OrderID, order.Symbol).Result;
        if (orderFound.IsSuccess)
            return true;
        return false;
    }
    private bool IsValidOrderToCancel(OrderEngine order)
    {
        var orderFound = _matchingCache.GetBuyOrderByIdandSymbolAsync(order.OrderID, order.Symbol).Result;

        if (orderFound.IsSuccess)
        {
            switch( orderFound.Value.OrderStatus)
            {
                case OrderStatus.Rejected:
                    return false;
                case OrderStatus.New:
                    return true;
                case OrderStatus.Cancelled:
                    return false;
                case OrderStatus.PartiallyFilled:
                    return true;
                case OrderStatus.Filled: 
                    return false;
            }
        }

        return false;
    }

    private void ValidateOrderExits()
    {
        RuleFor(o => o.Order)
            .Must(IsOrderExists)
            .WithMessage("1-Unknown order");
    }
    #endregion
}