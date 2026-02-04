using MatchingX.Application.Handlers;
using MatchingX.Application.Services;
using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Logging;
namespace MacthingX.Application.Services;
public sealed class MatchLimit : MatchBase
{
    private readonly IMediatorHandler Bus;
    public override string Name => nameof(MatchLimit);
    public MatchLimit(ILogger<MatchLimit> logger,
        IMediatorHandler bus,
        IMatchingRepository repository,
        IMatchingCache cache) : base(bus, repository, cache)
    {
        Bus = bus;
    }
}