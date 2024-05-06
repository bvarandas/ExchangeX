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
    PartiallyFilled= '1',
    Filled = '2',
    Cancelled ='4',
    Rejected='8',
}
public enum TimeInForce
{
    GTC,  // Good Till Cancel
    FOK,  // Fill or Kill
    DAY,  // Order will be automatically cancelled when trading day is closed
}

public enum RedisDataBases
{
    //OrderId = 0,
    Matching = 0,
    //MatchingTradeId= 1,             // é o execId do executionReport
    //MatchingExecutionReport = 2,    // dropcopy
    //MatchingExecutedTrade = 3,      // dropcopy
    //MatchingSecurity= 4,            // marketdata
    //MatchingSnapshotIncrement = 5,  // marketdata
    OrderEngine=1,                  // OrderEngine
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
    toCancel = 1,
    toCancelReplace = 2
}