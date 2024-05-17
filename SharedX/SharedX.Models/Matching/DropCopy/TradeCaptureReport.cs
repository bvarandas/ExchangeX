using ProtoBuf;
namespace SharedX.Core.Matching.DropCopy;

[ProtoContract]
public class TradeCaptureReport : DropCopyReport
{
    [ProtoMember(1)]
    public ushort TradeReportTransType { get; set; } // 0=new, 1=cancel
    [ProtoMember(2)]
    public ushort TrdType { get; set; } // only when new order 0=For order book trades, 1= For trade entry where type = Block
    [ProtoMember(3)]
    public char CopyMsgIndicator { get; set; } // indicates drop copy, always "Y"
    
    [ProtoMember(4)]
    public ushort NoSides {  get; set; } // always 1 (One Side)
    [ProtoMember(5)]
    public ushort TradeType { get; set;}
    [ProtoMember(6)]
    public string OrderId { get; set; }
    [ProtoMember(7)]
    public string ClOrderId { get; set; }
    [ProtoMember(8)]
    public decimal LastQty { get; set; }
    [ProtoMember(9)]
    public decimal LastPx { get; set; }
    [ProtoMember(10)]
    public string Symbol { get; set; }
    [ProtoMember(11)]
    public char Side { get; set; } // 1= Buy, 2= Sell
    [ProtoMember(12)]
    public decimal Price { get; set; } = 0;
    [ProtoMember(13)]
    public DateTime TransactTime { get; set; }
    [ProtoMember(14)]
    public string TradeDate { get; set; } = string.Empty;
    [ProtoMember(15)]
    public ushort AccountType { get; set; }
    [ProtoMember(16)]
    public bool Settlemented { get; set; }
    [ProtoMember(17)]
    public DateTime? SettlementDate { get; set; }
    [ProtoMember(18)]
    public char PreviouslyReported { get; set; }

    public TradeCaptureReport() { }

    public TradeCaptureReport(
        ushort tradeReportTransType, //0 - execution or trade 1-cancel
        ushort trdType, 
        char copyMsgIndicator,
        char previouslyReported,
        long tradeId, 
        ushort noSides, 
        ushort tradeType, 
        string orderId, 
        string clOrderId, 
        decimal lastQty, 
        decimal lastPx, 
        string symbol, 
        char side, 
        decimal price, 
        DateTime transactTime, 
        string tradeDate, 
        char accountType)
    {
        TradeReportTransType = tradeReportTransType;
        TrdType = trdType;
        CopyMsgIndicator = copyMsgIndicator;
        PreviouslyReported = previouslyReported;
        TradeId = tradeId;
        NoSides = noSides;
        TradeType = tradeType;
        OrderId = orderId;
        ClOrderId = clOrderId;
        LastQty = lastQty;
        LastPx = lastPx;
        Symbol = symbol;
        Side = side;
        Price = price;
        TransactTime = transactTime;
        TradeDate = tradeDate;
        AccountType = accountType;
    }
}