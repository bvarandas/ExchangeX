using MacthingX.Application.Events;
using MatchingX.Core.Repositories;
using MediatR;
using SharedX.Core.Bus;
namespace MacthingX.Application.Commands;
public class ExecutedTradeCommandHandler : IRequestHandler<ExecutedTradeCommand, bool>
{
    private readonly IExecutedTradeRepository _repository;
    private readonly IMediatorHandler _bus;

    public ExecutedTradeCommandHandler(IExecutedTradeRepository repository, IMediatorHandler bus)
    {
        _repository = repository;
        _bus = bus;
    }

    public async Task<bool> Handle(ExecutedTradeCommand command, CancellationToken cancellationToken)
    {
        await _repository.CreateExecutedTradeAsync(command.ExecutedTrade, cancellationToken);

        //if (Comit()) TODO: Fazer Unit of Work
        //{
        await _bus.RaiseEvent(new ExecutedTradeEvent(command.ExecutedTrade));
        //return await Task.FromResult(true);
        //}

        return await Task.FromResult(true);
    }
}
