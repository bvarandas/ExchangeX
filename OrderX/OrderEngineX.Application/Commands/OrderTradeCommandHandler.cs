using MarketDataX.Application.Commands;
using MediatR;
using SharedX.Core.Bus;
using SharedX.Core.Interfaces;
using OrderEngineX.Application.Events;
using OrderEngineX.Core.Notifications;
using OrderEngineX.Core.Interfaces;

namespace OrderEngineX.Application.Commands;
public class OrderTradeCommandHandler : CommandHandler,
    IRequestHandler<OrderTradeCancelCommand, bool>,
    IRequestHandler<OrderTradeModifyCommand, bool>,
    IRequestHandler<OrderTradeNewCommand, bool>
{
    private readonly IOrderRepository _repository;
    private readonly IMediatorHandler _bus;
    
    public OrderTradeCommandHandler(IOrderRepository repository, 
        IMediatorHandler bus, 
        INotificationHandler<DomainNotification> notifications) 
        : base(bus,notifications)  
    {
        _repository = repository;
        _bus = bus;
    }
    public async Task<bool> Handle(OrderTradeCancelCommand command, CancellationToken cancellationToken)
    {
        if (!command.IsValid())
        {
            NotifyValidationErrors(command);
            return false;
        }
        await _bus.RaiseEvent(new OrderTradeCancelEvent(command.Order));
        return true;
    }

    public async Task<bool> Handle(OrderTradeModifyCommand command, CancellationToken cancellationToken)
    {
        if (!command.IsValid())
        {
            NotifyValidationErrors(command);
            return false;
        }
        await _bus.RaiseEvent(new OrderTradeModifyEvent(command.Order));
        return true;
    }

    public async Task<bool> Handle(OrderTradeNewCommand command, CancellationToken cancellationToken)
    {
        if (!command.IsValid())
        {
            NotifyValidationErrors(command);
            return false;
        }

        var idOrder = _repository.GetOrderIdAsync(cancellationToken).Result;
        command.Order.OrderID = idOrder;

        await _repository.CreateOrdersAsync(command.Order, cancellationToken);
        await _bus.RaiseEvent(new OrderTradeNewEvent(command.Order));

        return true;
    }
}