using MacthingX.Application.Events;
using MatchingX.Core.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedX.Core.Bus;
namespace MacthingX.Application.Commands;
public class ExecutedTradeCommandHandler : IRequestHandler<ExecutedTradeCommand, bool>
{
    private readonly IExecutedTradeRepository _repository;
    private readonly IMediatorHandler _bus;
    private readonly ILogger<ExecutedTradeCommandHandler> _logger;
    public ExecutedTradeCommandHandler(
        IExecutedTradeRepository repository, 
        IMediatorHandler bus,
        ILogger<ExecutedTradeCommandHandler> _logger
        )
    {
        _repository = repository;
        _bus = bus;
    }

    public async Task<bool> Handle(ExecutedTradeCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Registrando execução de {command.ExecutedTrades.Count} ordens");
        await _repository.CreateExecutedTradeAsync(command.ExecutedTrades, cancellationToken);
        await _bus.RaiseEvent(new ExecutedTradeEvent(command.ExecutedTrades));
        
        return await Task.FromResult(true);
    }
}