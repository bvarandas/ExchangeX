using MacthingX.Application.Events;
using MediatR;
using SharedX.Core.Bus;
using SharedX.Core.Interfaces;
using SharedX.Core.Enums;
using MatchingX.Application.Commands;
using MatchingX.Core.Notifications;

namespace MacthingX.Application.Commands;

public class OrderCommandHandler :
    CommandHandler,
    IRequestHandler<OrderOpenedCommand, bool>,
    IRequestHandler<OrderCancelCommand, bool>,
    IRequestHandler<OrderCancelReplaceCommand, bool>,
    IRequestHandler<OrderTradeCommand, bool>
{
    private readonly IOrderRepository _repository;
    private readonly IMediatorHandler _bus;

    public OrderCommandHandler(IOrderRepository repository, 
        IMediatorHandler bus,
        INotificationHandler<DomainNotification> notifications)
        :base(bus, notifications)
    {
        _repository = repository;
        _bus = bus;
    }

    public async Task<bool> Handle(OrderCancelCommand command, CancellationToken cancellationToken)
    {
        if (!command.IsValid())
        {
            NotifyValidationErrors(command);
            return await Task.FromResult(false);
        }

        await _repository.UpdateOrderAsync(command.Order, cancellationToken);

        await _bus.RaiseEvent(new OrderCanceledEvent(command.Order));

        return await Task.FromResult(true);
    }

    public async Task<bool> Handle(OrderOpenedCommand command, CancellationToken cancellationToken)
    {
        if (!command.IsValid())
        {
            NotifyValidationErrors(command);
            return await Task.FromResult(false);
        }

        await _repository.UpdateOrderAsync(command.Order, cancellationToken);

        await _bus.RaiseEvent(new OrderOpenedEvent(command.Order));

        return await Task.FromResult(true);
    }

    public async Task<bool> Handle(OrderCancelReplaceCommand command, CancellationToken cancellationToken)
    {
        if (!command.IsValid())
        {
            NotifyValidationErrors(command);
            return await Task.FromResult(false);
        }
        await _repository.UpdateOrderAsync(command.Order, cancellationToken);

        await _bus.RaiseEvent(new OrderCanceledEvent(command.Order));

        return await Task.FromResult(true);
    }

    public async Task<bool> Handle(OrderTradeCommand command, CancellationToken cancellationToken)
    {
        if (!command.IsValid())
        {
            NotifyValidationErrors(command);
            return await Task.FromResult(false);
        }

        await _repository.UpdateOrderAsync(command.Order, cancellationToken);

        await _bus.RaiseEvent(new OrderTradedEvent(command.Order));
        
        return await Task.FromResult(true);
    }
}
