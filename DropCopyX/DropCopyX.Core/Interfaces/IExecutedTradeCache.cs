using SharedX.Core.Proto;
namespace DropCopyX.Core.Interfaces;
public interface IExecutedTradeCache
{
    long GetLastTradeId();
    void SerLastTradeId(long tradeId);
    void AddExecutionReport(ExecutedTrade trade);
}