using FluentResults;
using MediatR;
using OrderEntryX.Core.Repositories;
using SharedX.Core.Bus;
namespace OrderEntryX.Application.Commands;
public class OrderCommandHandler :
    IRequestHandler<NewOrderSingleCommand, Result>,
    IRequestHandler<OrderCancelRequestCommand, Result>,
    IRequestHandler<OrderCancelReplaceRequestCommand, Result>,
    IRequestHandler<OrderMassCancelRequestCommand, Result>
{
    private readonly IOrderEntryRepository _repository;
    private readonly IMediatorHandler _bus;

    public OrderCommandHandler(IOrderEntryRepository repository, IMediatorHandler bus)
    {
        _repository = repository;
        _bus = bus;
    }

    public async Task<Result> Handle(NewOrderSingleCommand command, CancellationToken cancellationToken)
    {
        var result = await _repository.CreateOrdersAsync(command.Order, cancellationToken);

        if (result.IsSuccess)
        {
            //await _bus.Publish()
        }
        //if (Comit()) TODO: Fazer Unit of Work
        //{
        //await _bus.RaiseEvent(new OrderFilledEvent(command.Order));
        //}

        return result;
    }

    public async Task<Result> Handle(OrderCancelRequestCommand command, CancellationToken cancellationToken)
    {
        var result = await _repository.CreateOrdersAsync(command.Order, cancellationToken);

        //if (Comit()) TODO: Fazer Unit of Work
        //{
        //await _bus.RaiseEvent(new OrderFilledEvent(command.Order));
        //}

        return result;
    }

    public async Task<Result> Handle(OrderCancelReplaceRequestCommand command, CancellationToken cancellationToken)
    {
        var result = await _repository.CreateOrdersAsync(command.Order, cancellationToken);

        //if (Comit()) TODO: Fazer Unit of Work
        //{
        //await _bus.RaiseEvent(new OrderFilledEvent(command.Order));
        //}

        return result;
    }

    public async Task<Result> Handle(OrderMassCancelRequestCommand command, CancellationToken cancellationToken)
    {
        var result = await _repository.CreateOrdersAsync(command.Order, cancellationToken);

        //if (Comit()) TODO: Fazer Unit of Work
        //{
        //await _bus.RaiseEvent(new OrderFilledEvent(command.Order));
        //}

        return result;
    }
}