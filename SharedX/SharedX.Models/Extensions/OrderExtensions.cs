using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using SharedX.Core.Matching;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.Models;
namespace SharedX.Core.Extensions;
public static class OrderExtensions
{
    public static OrderEngine ToOrder(this OrderModel model)
    {
        var order = new OrderEngine();
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

    public static OrderEngine ToOrder(this NewOrderSingle newOrder, SessionID sessionID)
    {
        var order = new OrderEngine();

        var stopPrice = newOrder.StopPx;

        if (stopPrice.Obj !=0)
            order.StopPrice = newOrder.StopPx.getValue();

        order.AccountId =long.Parse( newOrder.Account.getValue());
        order.ClOrdID = long.Parse( newOrder.ClOrdID.getValue());
        order.Quantity = newOrder.OrderQty.getValue();
        order.OrderType =(Enums.OrderType)Enum.Parse(typeof(Enums.OrderType),   newOrder.OrdType.getValue().ToString());
        order.TimeInForce = (Enums.TimeInForce)Enum.Parse(typeof(Enums.TimeInForce), newOrder.TimeInForce.getValue().ToString());
        order.TransactTime = newOrder.TransactTime.getValue();
        

        if (newOrder.IsSetExpireDate())
            order.ExpireDate = newOrder.ExpireDate.getValue();
        
        if (newOrder.IsSetExpireTime())
            order.ExpireTime =  newOrder.ExpireTime.getValue().ToString();
        
        //order.SessionID = sessionID;
        order.Side = (Enums.SideTrade)Enum.Parse(typeof(Enums.SideTrade), newOrder.Side.getValue().ToString());
        order.Symbol = newOrder.Symbol.getValue();

        PartyID partyId = new PartyID();
        PartyIDSource partyIDSource = new PartyIDSource();
        PartyRole partyRole = new PartyRole();

        var group = new NewOrderSingle.NoPartyIDsGroup();
        newOrder.GetGroup(1, group);
        group.Get(partyId);
        group.Get(partyIDSource);
        group.Get(partyRole);

        order.ParticipatorId = long.Parse(partyId.getValue());

        return order;
    }


}