namespace SharedX.Core;
public static class KeyNameRedis
{
    public const string TradeId = nameof(TradeId);
    public const string ExecutedTrade = nameof(ExecutedTrade);
    public const string ExecutionReport = nameof(ExecutionReport);
    public const string OrderEngine = nameof(OrderEngine);
    public const string DropCopyFixSession = nameof(DropCopyFixSession);
    public const string MarketDataFixSession = nameof(MarketDataFixSession);
}

public static class RequestTypeSecurity
{
    public const string List = nameof(List);
    public const string Status = nameof(Status);
}

public static class OutboxActivities
{
    public const string OrderEntryToOrderEngineSent = nameof(OrderEntryToOrderEngineSent);
    public const string OrderEngineToMatchingSent= nameof(OrderEngineToMatchingSent);

    public const string MatchingToOrderEngineSent= nameof(MatchingToOrderEngineSent);
    public const string MatchingToDropCopySent = nameof(MatchingToDropCopySent);
    public const string MatchingToMarketDataSent = nameof(MatchingToMarketDataSent);

    public const string SecurityEngineToMatchingSent = nameof(SecurityEngineToMatchingSent);
    public const string SecurityEngineToMarketDataSent = nameof(SecurityEngineToMarketDataSent);
}