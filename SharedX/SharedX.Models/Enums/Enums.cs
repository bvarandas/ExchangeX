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
}

public enum RedisDataBases
{
    OrderId = 0,
    MatchingTradeId= 1, // é o execId do executionReport
    MatchingExecutionReport = 2,
    MatchingExecutedTrade = 3
}