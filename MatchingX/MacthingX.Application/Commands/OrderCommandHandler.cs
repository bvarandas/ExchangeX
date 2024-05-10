using MacthingX.Application.Events;
using MediatR;
using SharedX.Core.Bus;
using SharedX.Core.Interfaces;

namespace MacthingX.Application.Commands;

public class OrderCommandHandler :
    IRequestHandler<OrderOpenedCommand, bool>,
    IRequestHandler<OrderCancelCommand, bool>,
    IRequestHandler<OrderCancelReplaceCommand, bool>
{
    private readonly IOrderRepository _repository;
    private readonly IMediatorHandler _bus;

    public OrderCommandHandler(IOrderRepository repository, IMediatorHandler bus)
    {
        _repository = repository;
        _bus = bus;
    }

    public async Task<bool> Handle(OrderCancelCommand command, CancellationToken cancellationToken)
    {
        await _repository.UpdateOrderAsync(command.Order, cancellationToken);

        //if (Comit()) TODO: Fazer Unit of Work
        //{
        await _bus.RaiseEvent(new OrderCanceledEvent(command.Order));
        //}

        return await Task.FromResult(true);
    }

    public async Task<bool> Handle(OrderOpenedCommand command, CancellationToken cancellationToken)
    {
        await _repository.UpdateOrderAsync(command.Order, cancellationToken);

        //if (Comit()) TODO: Fazer Unit of Work
        //{
        await _bus.RaiseEvent(new OrderCanceledEvent(command.Order));
        //}

        return await Task.FromResult(true);
    }

    public async Task<bool> Handle(OrderCancelReplaceCommand command, CancellationToken cancellationToken)
    {
        await _repository.UpdateOrderAsync(command.Order, cancellationToken);

        //if (Comit()) TODO: Fazer Unit of Work
        //{
        await _bus.RaiseEvent(new OrderCanceledEvent(command.Order));
        //}

        return await Task.FromResult(true);
    }
}
