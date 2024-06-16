using FluentValidation;
using OrderEngineX.Application.Commands;
using OrderEngineX.Core.Interfaces;
using SharedX.Core.Entities;
using SharedX.Core.Enums;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;
using System.Diagnostics.SymbolStore;
using static StackExchange.Redis.Role;

namespace MacthingX.Application.Validations;
public abstract class OrderValidation<T> : 
    AbstractValidator<T> where T: 
    OrderEngineCommand
{
    private readonly ISecurityEngineCache _securityEngineCache;
    private readonly IBookOfferCache _matchingCache;
    public OrderValidation(IBookOfferCache matchingCache, ISecurityEngineCache securityEngineCache)
    {
        _securityEngineCache = securityEngineCache;
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

        ValidateNewOrderSingleSymbol();
        ValidateNewOrderSingleOrderId();
        ValidateSecurityOrder();
        ValidateAccountId();
        ValidateParticipatorId();
        ValidateStopPx();
        ValidatePriceLimit();
        ValidatePriceMarket();
        ValidateTimeInForceFOK();
        ValidateTimeInForceGTC();
        ValidateTimeInForceIOC();
        ValidateSecurityLowPriceLimitOrder();
        ValidateSecurityHighPriceLimitOrder();
        ValidateSecurityMaxTradeVolume();
        ValidateSecurityMinTradeVolume();
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
        ValidateSecurityOrder();
        ValidateOrderToLateToCancel();
        ValidateOrderCancelOrderId();
        ValidateOrderExits();
        ValidateOrderDuplicateClOrdID();
    }
    protected void ValidateReplaceOrder()
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
        ValidateReplaceClOrdID();
        ValidateReplaceSymbol();
        ValidateSecurityOrder();
        ValidateReplaceOrderId();
        ValidateReplaceAccountId();
        ValidateReplaceParticipatorId();
        ValidateStopPx();
        ValidatePriceLimit();
        ValidatePriceMarket();
        ValidateReplaceTimeInForce();
    }

    protected void ValidateCancelReplaceOrder()
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
        ValidateSecurityOrder();
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
        ValidateTimeInForceFOK();
        ValidateTimeInForceGTC();
        ValidateTimeInForceIOC();
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
    private void ValidateNewOrderSingleSymbol()
    {
        RuleFor(o => o.Order.Symbol)
            .NotEqual(string.Empty)
            .WithName("neworder")
            .WithMessage("2-Unknown Security")
            .WithErrorCode("2");
    }
    private void ValidateReplaceSymbol()
    {
        RuleFor(o => o.Order.Symbol)
            .NotEqual(string.Empty)
            .WithName("replace")
            .WithMessage("2-Unknown Security")
            .WithErrorCode("2");
    }
    private void ValidateReplaceClOrdID()
    {
        RuleFor(o => o.Order.ClOrdID)
            .NotEqual(0)
            .WithName("replace")
            .WithMessage("5-Invalid ClOrderID order")
            .WithErrorCode("5");
    }
    private void ValidateNewOrderSingleOrderId()
    {
        RuleFor(o => o.Order.OrderID)
            .NotEqual(0)
            .WithName("neworder")
            .WithMessage("5-Invalid OrderID order")
            .WithErrorCode("5"); 
    }
    private void ValidateReplaceOrderId()
    {
        RuleFor(o => o.Order.OrderID)
            .NotEqual(0)
            .WithName("replace")
            .WithMessage("5-Invalid OrderID order")
            .WithErrorCode("5");
    }
    private void ValidateReplaceAccountId()
    {
        RuleFor(o => o.Order.AccountId)
            .NotEqual(0)
            .WithName("replace")
            .WithMessage("5-Invalid accountId order")
            .WithErrorCode("5"); 
    }
    private void ValidateAccountId()
    {
        RuleFor(o => o.Order.AccountId)
            .NotEqual(0)
            .WithMessage("5-Invalid accountId order")
            .WithErrorCode("5");
    }
    private void ValidateParticipatorId()
    {
        RuleFor(o => o.Order.ParticipatorId)
            .NotEqual(0)
            .WithName("neworder")
            .WithMessage("5-Invalid ParticipatorId order")
            .WithErrorCode("5");
    }
    private void ValidateReplaceParticipatorId()
    {
        RuleFor(o => o.Order.ParticipatorId)
            .NotEqual(0)
            .WithName("replace")
            .WithMessage("5-Invalid ParticipatorId order")
            .WithErrorCode("5");
    }
    private void ValidatePriceLimit()
    {
        RuleFor(o => o.Order.Price)
            .Must(p=>p>0)
            .When(o=>o.Order.OrderType == OrderType.Limit || o.Order.OrderType== OrderType.StopLimit)
            .WithMessage("5-Invalid Price for limit or stopLimit order")
            .WithErrorCode("5"); 
    }
    private void ValidatePriceMarket()
    {
        RuleFor(o => o.Order.Price)
            .Must(p => p == 0)
            .When(o => o.Order.OrderType == OrderType.Market || o.Order.OrderType == OrderType.Stop)
            .WithMessage("5-Invalid Price for market or stop order, must be 0 (zero)!")
            .WithErrorCode("5");
    }
    private void ValidateStopPx()
    {
        RuleFor(o => o.Order.StopPrice)
            .Must(p => p > 0)
            .When(p => p.Order.OrderType == OrderType.Stop || p.Order.OrderType == OrderType.StopLimit)
            .WithMessage("5-Invalid StopPx to order stop or stoplimit")
            .WithErrorCode("5"); 
    }
    private void ValidateTimeInForceFOK()
    {
        RuleFor(o => o.Order.TimeInForce)
            .Must(p => p== TimeInForce.FOK)
            .When(p => p.Order.OrderType == OrderType.Market|| p.Order.OrderType == OrderType.Stop)
            .WithMessage("5-Invalid TimeInForce FOK to order market or stop")
            .WithErrorCode("5");
    }

    private void ValidateTimeInForceGTC()
    {
        RuleFor(o => o.Order.TimeInForce)
            .Must(p => p == TimeInForce.GTC)
            .When(p => p.Order.OrderType == OrderType.Market || p.Order.OrderType == OrderType.Stop)
            .WithMessage("5-Invalid TimeInForce GTC to order market or stop")
            .WithErrorCode("5");
    }

    private void ValidateTimeInForceIOC()
    {
        RuleFor(o => o.Order.MinQty)
            .Must(p => p > 0)
            .When(p => p.Order.TimeInForce == TimeInForce.IOC)
            .WithMessage("5-Invalid MinQty to order with timeinforce IOC")
            .WithErrorCode("5");
    }

    private void ValidateReplaceTimeInForce()
    {
        RuleFor(o => o.Order.TimeInForce)
            .Must(p => p == TimeInForce.FOK)
            .When(p => p.Order.OrderType == OrderType.Market || p.Order.OrderType == OrderType.Stop)
            .WithName("replace")
            .WithMessage("5-Invalid TimeInForce to order market or stop")
            .WithErrorCode("5");
    }
    private void ValidateOrderCancelOrderId()
    {
        RuleFor(o => o.Order.OrderID)
            .NotEqual(0)
            .WithMessage("1-Invalid OrderID order")
            .WithErrorCode("1");
    }
    private void ValidateOrderToLateToCancel()
    {
        RuleFor(o => o.Order)
            .Must(IsValidOrderToCancel)
            .WithMessage("0-To Late to cancel")
            
            .WithErrorCode("0") ;
    }
    private void ValidateOrderDuplicateClOrdID()
    {
        RuleFor(o => o.Order)
            .Must(IsNotDuplicatedClOrdID)
            .WithMessage("6-Duplicate ClOrdID <11> received")
            .WithErrorCode("6"); 
    }

    private void ValidateSecurityOrder()
    {
        RuleFor(o => o.Order)
            .Must(IsSecurityExists)
            .WithMessage("2-Unknown Security: Symbol not found")
            .WithErrorCode("2");
    }
    private void ValidateSecurityLowPriceLimitOrder()
    {
        RuleFor(o => o.Order)
            .Must(IsNotPriceLessThanLowLimitPrice)
            .WithMessage("18-Invalid price increment: Order Price is less than low limit price security")
            .WithErrorCode("18");
    }

    private void ValidateSecurityHighPriceLimitOrder()
    {
        RuleFor(o => o.Order)
            .Must(IsNotPriceGreaterThanHighLimitPrice)
            .WithMessage("18-Invalid price increment: Order Price is greater than high limit price security")
            .WithErrorCode("18");
    }

    private void ValidateSecurityMaxTradeVolume()
    {
        RuleFor(o => o.Order)
            .Must(IsNotQuantitySecurityGreaterThanMaxTradeVol)
            .WithMessage("0-Other: Order Quantity is greater than Max Trade volume security")
            .WithErrorCode("18");
    }
    private void ValidateSecurityMinTradeVolume()
    {
        RuleFor(o => o.Order)
            .Must(IsNotQuantitySecurityLessThanMinTradeVol)
            .WithMessage("0-Other: Order Price is greater than high limit price security")
            .WithErrorCode("18");
    }
    private bool IsSecurityExists(OrderEngine order) =>
        _securityEngineCache.TryGetSecurity(order.Symbol, out SecurityEngine security);
        
    
    private bool IsNotPriceLessThanLowLimitPrice(OrderEngine order)
    {
        if (_securityEngineCache.TryGetSecurity(order.Symbol, out SecurityEngine security))
        { 
            if (security.LowLimitPrice > order.Price)
                return false;
        }
        return true;
    }

    private bool IsNotPriceGreaterThanHighLimitPrice( OrderEngine order)
    {
        if (_securityEngineCache.TryGetSecurity(order.Symbol, out SecurityEngine security))
        {
            if (security.HighLimitPrice < order.Price)
                return false;
        }
        return true;
    }

    private bool IsNotQuantitySecurityGreaterThanMaxTradeVol( OrderEngine order)
    {
        if (_securityEngineCache.TryGetSecurity(order.Symbol, out SecurityEngine security))
        {
            if (security.MaxTradeVol < order.Quantity)
                return false;
        }
        return true;
    }

    private bool IsNotQuantitySecurityLessThanMinTradeVol( OrderEngine order)
    {
        if ( _securityEngineCache.TryGetSecurity(order.Symbol, out SecurityEngine security))
        {
            if (security.MinTradeVol > order.Quantity)
                return false;
        }
        return true;
    }

    private bool IsOrderExists(OrderEngine order)
    {
        var orderFound = _matchingCache.GetBuyOrderByIdandSymbolAsync(order.OrderID, order.Symbol).Result;
        if (orderFound.IsSuccess)
        {

            return true;
        }
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
                case OrderStatus.Filled: 
                    return false;
                case OrderStatus.PartiallyFilled:
                    return true;
                case OrderStatus.PendingCancel:
                    return false;
                case OrderStatus.PendindReplace:
                    return false;

            }
        }

        return false;
    }

    private bool IsNotDuplicatedClOrdID(OrderEngine order)
    {
        var orders= _matchingCache.GetBuyOrderBySymbol(order.Symbol).Result;

        if (orders.IsSuccess)
        {
            var found = orders.Value.FirstOrDefault(o=>o.Value.ClOrdID==order.ClOrdID);
            if (found.Value is not null)
                return false;
        }

        return true;
    }

    private void ValidateOrderExits()
    {
        RuleFor(o => o.Order)
            .Must(IsOrderExists)
            .WithMessage("1-Unknown order")
            .WithErrorCode("1");
    }
    #endregion
}