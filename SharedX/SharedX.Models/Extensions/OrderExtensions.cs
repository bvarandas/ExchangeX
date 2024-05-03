using SharedX.Core.Matching;
using SharedX.Core.Models;
namespace SharedX.Core.Extensions;
public static class OrderExtensions
{
    public static Order ToOrder(this OrderModel model)
    {
        var order = new Order();
        order.OrderID = model.OrderID;
        order.StopPrice = model.StopPrice;
        order.LastPrice = model.Price;
        order.AveragePrice = model.Price;
        order.Price = model.Price;
        order.AccountId = model.Account;
        order.ClOrdID = model.ClOrdId;
        order.OrderStatus = model.OrderStatus;
        order.LastQuantity = model.Quantity;
        order.Quantity = model.Quantity;
        order.LeavesQuantity = 0;
        order.OrderType = model.OrderType;
        order.ParticipatorId = model.MpId;
        order.TimeInForce = model.TimeInForce;
        order.TransactTime = model.TransactTime;
        order.Side = model.Side;
        order.Symbol = model.Symbol;
        
        //order.Account = new Account.Limit() { AccountId = model.Account,  };

        return order;
    }
}