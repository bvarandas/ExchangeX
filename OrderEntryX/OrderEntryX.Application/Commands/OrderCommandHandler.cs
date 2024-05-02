using MediatR;
using OrderEntryX.Core.Repositories;
using SharedX.Core.Bus;
namespace OrderEntryX.Application.Commands;
public class OrderCommandHandler :
    IRequestHandler<NewOrderSingleCommand, bool>,
    IRequestHandler<OrderCancelRequestCommand, bool>,
    IRequestHandler<OrderCancelReplaceRequestCommand, bool>,
    IRequestHandler<OrderMassCancelRequestCommand, bool>
{
    private readonly IOrderEntryRepository _repository;
    private readonly IMediatorHandler _bus;

    public OrderCommandHandler(IOrderEntryRepository repository, IMediatorHandler bus)
    {
        _repository = repository;
        _bus = bus;
    }

    public async Task<bool> Handle(NewOrderSingleCommand command, CancellationToken cancellationToken)
    {
        await _repository.CreateOrdersAsync(command.Order, cancellationToken);

        //if (Comit()) TODO: Fazer Unit of Work
        //{
        //await _bus.RaiseEvent(new OrderFilledEvent(command.Order));
        //}

        return await Task.FromResult(true);
    }

    public async Task<bool> Handle(OrderCancelRequestCommand command, CancellationToken cancellationToken)
    {
        await _repository.CreateOrdersAsync(command.Order, cancellationToken);

        //if (Comit()) TODO: Fazer Unit of Work
        //{
        //await _bus.RaiseEvent(new OrderFilledEvent(command.Order));
        //}

        return await Task.FromResult(true);
    }

    public async Task<bool> Handle(OrderCancelReplaceRequestCommand command, CancellationToken cancellationToken)
    {
        await _repository.CreateOrdersAsync(command.Order, cancellationToken);

        //if (Comit()) TODO: Fazer Unit of Work
        //{
        //await _bus.RaiseEvent(new OrderFilledEvent(command.Order));
        //}

        return await Task.FromResult(true);
    }

    public async Task<bool> Handle(OrderMassCancelRequestCommand command, CancellationToken cancellationToken)
    {
        await _repository.CreateOrdersAsync(command.Order, cancellationToken);

        //if (Comit()) TODO: Fazer Unit of Work
        //{
        //await _bus.RaiseEvent(new OrderFilledEvent(command.Order));
        //}

        return await Task.FromResult(true);
    }
}