using FluentResults;
using MacthingX.Application.Commands;
using MacthingX.Application.Events;
using MatchingX.Core.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedX.Core.Bus;
namespace MacthingX.Application.Handlers;
public class ExecutedTradeCommandHandler : IRequestHandler<ExecutedTradeCommand, Result>
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

    public async Task<Result> Handle(ExecutedTradeCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Registrando execução de {command.ExecutedTrades.Count} ordens");
        var result = await _repository.CreateExecutedTradeAsync(command.ExecutedTrades, cancellationToken);
        if (result.IsSuccess)
            await _bus.Publish(new ExecutedTradeEvent(command.ExecutedTrades));

        return result;
    }
}