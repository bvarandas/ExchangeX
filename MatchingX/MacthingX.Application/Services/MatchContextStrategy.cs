using MatchingX.Core.Entities;
using MatchingX.Core.Interfaces;

namespace MacthingX.Application.Services;
public sealed class MatchContextStrategy : IMatchContextStrategy
{
    private readonly IEnumerable<IMatch> _matchList;
    private IMatchingCache _matchingCache;
    private IMatch _actualMatch;

    public MatchContextStrategy(IEnumerable<IMatch> matchList,
        IMatchingCache matchingCache)
    {
        this._matchList = matchList;
        this._matchingCache = matchingCache;
    }

    public bool SetStrategy(string strategyName)
    {
        var instance = _matchList.FirstOrDefault(x =>
            x.Name.Equals(strategyName, StringComparison.InvariantCultureIgnoreCase));

        _actualMatch = instance!;

        return true;
    }
    public async void ReceivedOrder(MatchingEngine order, CancellationToken cancellationToken)
    {
        await this._actualMatch.ReceiveOrderAsync(order.PrincipalOrder, cancellationToken);
        await this.MatchOrderAsync(order, cancellationToken);
    }
    public async Task<bool> MatchOrderAsync(MatchingEngine order, CancellationToken cancellationToken)
        => await this._actualMatch.MatchOrderAsync(order.PrincipalOrder, cancellationToken);


    public async Task<bool> CancelOrderAsync(MatchingEngine order, CancellationToken cancellationToken)
        => await this._actualMatch.CancelOrderAsync(order.PrincipalOrder, cancellationToken);
}