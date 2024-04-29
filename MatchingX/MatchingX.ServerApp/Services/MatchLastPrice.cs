using SharedX.Core.Proto;
using System.Collections.Concurrent;

namespace MacthingX.FixApp.Services;
public abstract class MatchLastPrice
{
    protected readonly ConcurrentDictionary<string, decimal> _lastPrice;

    public MatchLastPrice()
    {
        _lastPrice= new ConcurrentDictionary<string, decimal>();
    }

    protected void AddUpdatePrice(ExecutedTrade trade)
    {
        if (_lastPrice.TryGetValue(trade.Symbol, out decimal price))
        {
            _lastPrice[trade.Symbol] = price;
        }else
            _lastPrice.TryAdd(trade.Symbol, trade.OrderPrice);
    }
}