using MediatR;
using SharedX.Core.Bus;
using SharedX.Core.Interfaces;
using OrderEngineX.Application.Events;
using OrderEngineX.Core.Notifications;
using OrderEngineX.Application.Commands.Order;
namespace OrderEngineX.Application.Commands;
public class OrderEngineCommandHandler : CommandHandler,
    IRequestHandler<OrderCancelCommand, bool>,
    IRequestHandler<OrderCancelReplaceCommand, bool>,
    IRequestHandler<OrderOpenedCommand, bool>
{
    private readonly IOrderRepository _repository;
    private readonly IMediatorHandler _bus;
    public OrderEngineCommandHandler(IOrderRepository repository, 
        IMediatorHandler bus, 
        INotificationHandler<DomainNotification> notifications) 
        : base(bus,notifications)  
    {
        _repository = repository;
        _bus = bus;
    }
    
    public async Task<bool> Handle(OrderCancelCommand command, CancellationToken cancellationToken)
    {
        if (!command.IsValid())
        {
            NotifyValidationErrors(command);
            return false;
        }
        await _bus.RaiseEvent(new OrderTradeCancelEvent(command.Order));
        return true;
    }

    public async Task<bool> Handle(OrderCancelReplaceCommand command, CancellationToken cancellationToken)
    {
        if (!command.IsValid())
        {
            NotifyValidationErrors(command);
            return false;
        }
        //// Primeiro, verificar se a ordem é igual e só muda 
        //var idOrder = _repository.GetOrderIdAsync(cancellationToken).Result;
        //command.Order.OrderID = idOrder;

        await _bus.RaiseEvent(new OrderTradeModifyEvent(command.Order));
        return true;
    }

    public async Task<bool> Handle(OrderOpenedCommand command, CancellationToken cancellationToken)
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