namespace SharedX.Core;
public static class Constants
{
    public const string RedisKeyTradeId = nameof(RedisKeyTradeId);
    public const string RedisExecutedTrade = nameof(RedisExecutedTrade);
    public const string RedisKeyExecutionReport = nameof(RedisKeyExecutionReport);
    public const string RedisOrderEngine = nameof(RedisOrderEngine);
    public const string RedisDropCopyFixSession = nameof(RedisDropCopyFixSession);
}

public static class OutboxActivities
{
    public const string OrderEngineToMatchingSent= nameof(OrderEngineToMatchingSent);
    public const string OrderEntryToOrderEngineSent = nameof(OrderEntryToOrderEngineSent);

    public const string MatchingToOrderEngineSent= nameof(MatchingToOrderEngineSent);
    public const string MatchingToDropCopySent = nameof(MatchingToDropCopySent);
    public const string MatchingToMarketDataSent = nameof(MatchingToMarketDataSent);
}