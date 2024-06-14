namespace SharedX.Core.Enums;

public enum SideTrade
{
    Buy ='1',
    Sell='2'
}
public enum OrderType
{
    Market='1',
    Limit='2',
    Stop ='3',
    StopLimit = '4'
}
public enum OrderStatus
{
    New = '0',
    PartiallyFilled ='1',
    Filled='2',
    Cancelled ='4',
    PendingCancel='6',
    Rejected='8',
    PendindReplace = 'E',

}
public enum TimeInForce
{
    GTC,  // Good Till Cancel
    FOK,  // Fill or Kill,
    IOC,  // Immediate Or Cancelled (allows partial fills)
    DAY,  // Order will be automatically cancelled when trading day is closed
}

public enum RedisDataBases
{
    Matching = 0,
    OrderEngine=1,
    OrderReport=2,
    Security=3,
    Fix = 4,    // DropCopy e MarketData
    Outbox = 5,
    Trade = 6
    
}
//public enum Redis
public enum TradeReportTransType
{
    Trade = 0,
    Cancellation =1
}

public enum Execution
{
    ToOpen = 0,
    ToCancel = 1,
    ToCancelReplace = 2,
    ToModify = 3
}