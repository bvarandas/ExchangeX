using SharedX.Core.Enums;
using SharedX.Core.Matching;
using SharedX.Core.Matching.DropCopy;
using SharedX.Core.Matching.MarketData;
using SharedX.Core.Matching.OrderEngine;

namespace MacthingX.Application.Extensions;
public static class MatchingExtensions 
{
     public static MarketData ToMarketData(this OrderEngine order)
    {
        var marketdata = new MarketData();
        marketdata.Symbol = order.Symbol;
        marketdata.SecurityID = "";
        marketdata.SecuritSourceId = '8';
        marketdata.EntryID = (order.Side == SideTrade.Buy ? '0' : '1');
        marketdata.EntryType = (order.OrderStatus == OrderStatus.Filled || order.OrderStatus == OrderStatus.PartiallyFilled) ? '2' : (order.Side == SideTrade.Buy ? '0' : '1');
        marketdata.EntryPx = order.Price;
        marketdata.EntrySize = order.Quantity;
        marketdata.EntryDate = order.TransactTime.ToString("yyyyMMdd");
        marketdata.EntryTime = order.TransactTime.ToString("HH:mm:ss.zzz");
        marketdata.QuoteCondition = "";
        marketdata.TradeCondition = "";
        marketdata.QuoteType = "";
        marketdata.AggressorSide =(char) order.Side;
        return marketdata;
    }
    public static ExecutionReport ToExecutionReport(this OrderEngine order)
    {
        var isTrade=  (order.OrderStatus == OrderStatus.Filled || order.OrderStatus== OrderStatus.PartiallyFilled);
        var ep = new ExecutionReport();
        ep.OrderID = order.OrderID;
        ep.OrigCLOrdID = order.ClOrdID;
        ep.ClOrdID = order.ClOrdID;
        ep.ExecID = 0;
        ep.TradeId = order.OrderID;                       
        ep.ExecType = (char)order.OrderStatus;
        ep.OrderStatus = order.OrderStatus;
        ep.OrdRejReason = "";                   // verificar depois na rejeição
        ep.Quantity = order.Quantity;
        ep.CumQty = 0;                          //verificar depois no trade
        ep.LeavesQuantity = order.LeavesQuantity;
        ep.LastQuantity = order.LastQuantity;
        ep.ExpireDate = order.ExpireDate;
        ep.ExpireTime = order.ExpireTime;
        ep.AccoutType = '1';

        return ep;
    }
}