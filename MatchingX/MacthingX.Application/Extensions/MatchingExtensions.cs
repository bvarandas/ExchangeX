using MatchingX.Core.Entities;
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
        marketdata.AggressorSide = (char)order.Side;
        return marketdata;
    }
    public static ExecutionReport ToExecutionReport(this OrderEngine order)
    {
        var isTrade = (order.OrderStatus == OrderStatus.Filled || order.OrderStatus == OrderStatus.PartiallyFilled);
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
        ep.AccountType = '1';

        return ep;
    }
    public static Book ToBookItem(this OrderEngine order)
        => new Book()
        {
            Side = order.Side,
            Price = order.Price,
            Symbol = order.Symbol,
            OrderId = order.OrderID,
            Amount = order.Quantity,
            Timestamp = order.TransactTime
        };
    public static MatchingEngine ToMatching(this OrderEngine order)
    {
        var matchingEngine = new MatchingEngine();
        matchingEngine.PrincipalOrder = new MatchOrder
        {
            OrderID = order.OrderID,
            Symbol = order.Symbol,
            Quantity = order.Quantity,
            LeavesQuantity = order.LeavesQuantity,
            TimeInForce = order.TimeInForce,
            Side = order.Side,
            OrderType = order.OrderType,
            OrderStatus = order.OrderStatus,
            Execution = order.Execution,
            ClOrdID = order.ClOrdID,
            AccountId = order.AccountId,
            Price = order.Price,
            StopPrice = order.StopPrice,
            TransactTime = order.TransactTime,
            OrigClOrdID = order.OrigClOrdID,
            LastQuantity = order.LastQuantity,
            LastPrice = order.LastPrice
        };

        return matchingEngine;
    }
    public static MarketData ToMarketData(this MatchOrder order)
        => new MarketData()
        {
            Symbol = order.Symbol,
            SecurityID = "",
            SecuritSourceId = '8',
            EntryID = (order.Side == SideTrade.Buy ? '0' : '1'),
            EntryType = (order.OrderStatus == OrderStatus.Filled || order.OrderStatus == OrderStatus.PartiallyFilled) ? '2' : (order.Side == SideTrade.Buy ? '0' : '1'),
            EntryPx = order.Price,
            EntrySize = order.Quantity,
            EntryDate = order.TransactTime.ToString("yyyyMMdd"),
            EntryTime = order.TransactTime.ToString("HH:mm:ss.zzz"),
            QuoteCondition = "",
            TradeCondition = "",
            QuoteType = "",
            AggressorSide = (char)order.Side
        };
    public static Dictionary<long, TradeReport> ToExecutionReport(this MatchingEngine matchingEngine)
    {
        var result = new Dictionary<long, TradeReport>();

        var order = matchingEngine.PrincipalOrder;

        var report = new ExecutionReport();
        report.AccountType = order.AccountType; //1= Client and 3 = House
        report.TimeInForce = order.TimeInForce;
        report.StopPrice = order.StopPrice;
        report.Symbol = order.Symbol;
        report.Quantity = order.Quantity;
        report.Side = order.Side;
        report.OrigCLOrdID = order.ClOrdID;
        report.OrderID = order.OrderID;
        report.TradeId = order.OrderID;
        report.ExecID = order.OrderID;
        report.Price = order.Price;
        report.ExecType = 'F';      // Fully or partially 

        result.Add(report.TradeId, report);

        foreach (var orderPart in matchingEngine.PartOrders.Values)
        {
            var reportPart = new ExecutionReport();
            reportPart.AccountType = order.AccountType; //1= Client and 3 = House
            reportPart.TimeInForce = orderPart.TimeInForce;
            reportPart.StopPrice = orderPart.StopPrice;
            reportPart.Symbol = orderPart.Symbol;
            reportPart.Quantity = orderPart.Quantity;
            reportPart.Side = orderPart.Side;
            reportPart.OrigCLOrdID = orderPart.ClOrdID;
            reportPart.OrderID = orderPart.OrderID;
            reportPart.TradeId = order.OrderID;
            reportPart.ExecID = order.OrderID;
            reportPart.Price = orderPart.Price;
            reportPart.ExecType = 'F';      // Fully or partially 

            result.Add(reportPart.TradeId, reportPart);
        }

        return result;
    }
    public static Dictionary<long, TradeReport> ToCaptureReport(this MatchingEngine matchingEngine)
    {
        var result = new Dictionary<long, TradeReport>();

        var order = matchingEngine.PrincipalOrder;

        var report = new TradeCaptureReport()
        {
            TradeReportTransType = 0,
            TrdType = 0,
            CopyMsgIndicator = 'Y',
            PreviouslyReported = 'N',
            TradeId = order.OrderID,
            NoSides = 1,
            OrderId = order.OrderID.ToString(),
            ClOrderId = order.ClOrdID.ToString(),
            LastQty = order.LastQuantity,
            LastPx = order.LastPrice,
            Symbol = order.Symbol,
            Side = (char)order.Side,
            Price = order.Price,
            TransactTime = order.TransactTime,
            TradeDate = DateTime.Now.ToString("yyyyMMdd"),
        };

        result.Add(report.TradeId, report);

        foreach (var orderPart in matchingEngine.PartOrders.Values)
        {
            var reportPart = new TradeCaptureReport()
            {
                TradeReportTransType = 0,
                TrdType = 0,
                CopyMsgIndicator = 'Y',
                PreviouslyReported = 'N',
                TradeId = order.OrderID,
                NoSides = 1,
                OrderId = orderPart.OrderID.ToString(),
                ClOrderId = orderPart.ClOrdID.ToString(),
                LastQty = orderPart.LastQuantity,
                LastPx = orderPart.LastPrice,
                Symbol = orderPart.Symbol,
                Side = (char)orderPart.Side,
                Price = orderPart.Price,
                TransactTime = orderPart.TransactTime,
                TradeDate = DateTime.Now.ToString("yyyyMMdd"),
            };

            result.Add(reportPart.TradeId, reportPart);
        }

        return result;
    }
}