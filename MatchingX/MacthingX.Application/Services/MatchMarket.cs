using MatchingX.Application.Handlers;
using MatchingX.Application.Services;
using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace MacthingX.Application.Services;
public sealed class MatchMarket : MatchBase
{
    private readonly IMediatorHandler Bus;
    public override string Name => nameof(MatchMarket);

    public MatchMarket(ILogger<MatchMarket> logger, IMediatorHandler bus, IMatchingRepository repository) : base(bus, repository)
    {
        Bus = bus;
    }
}