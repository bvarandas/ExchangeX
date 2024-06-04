using MarketDataX.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedX.Core.Bus;
namespace MarketDataX.Application.Commands;
public class SnapshotCommandHandler : IRequestHandler<SnapshotCommand, bool>
{
    private readonly IMarketDataRepository _marketDataRepository = null!;
    private readonly IMediatorHandler _bus = null!;
    private readonly ILogger<SnapshotCommandHandler> _logger = null!;

    public SnapshotCommandHandler(IMarketDataRepository marketDataRepository, IMediatorHandler bus, ILogger<SnapshotCommandHandler> logger)
    {
        _marketDataRepository = marketDataRepository;
        _bus = bus;
        _logger = logger;

    }
    public async Task<bool> Handle(SnapshotCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Command de para gravar Snapshot de marketdata ");

        var result = await _marketDataRepository.UpsertMarketDataSnapshotAsync(command.Snapshot, cancellationToken);

        return result;
    }
}