namespace SharedX.Core;
public static class Constants
{
    public const string RedisKeyTradeId = nameof(RedisKeyTradeId);
    public const string RedisExecutedTrade = nameof(RedisExecutedTrade);
    public const string RedisExecutionReport = nameof(RedisExecutionReport);
    public const string RedisOrderEngine = nameof(RedisOrderEngine);
}

public static class OutboxActivities
{
    public const string OrderEngineReceived= nameof(OrderEngineReceived);
    public const string MatchingReceived = nameof(MatchingReceived);

    //public const string OrderPartiallyFilled = nameof(OrderPartiallyFilled);
    //public const string OrderCancelled = nameof(OrderCancelled);
    //public const string OrderRejected= nameof(OrderRejected);
}