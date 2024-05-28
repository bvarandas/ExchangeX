using MediatR;
using SharedX.Core.Bus;
using SharedX.Core.Interfaces;
using OrderEngineX.Application.Events;
using OrderEngineX.Core.Notifications;
using OrderEngineX.Application.Commands.Order;
using SharedX.Core.Matching.OrderEngine;
using Microsoft.Extensions.Logging;
namespace OrderEngineX.Application.Commands;
public class OrderEngineCommandHandler : CommandHandler,
    IRequestHandler<OrderCancelCommand, bool>,
    IRequestHandler<OrderCancelReplaceCommand, bool>,
    IRequestHandler<OrderOpenedCommand, bool>
{
    private readonly ILogger<OrderEngineCommandHandler> _logger;
    private readonly IOrderRepository _repository;
    private readonly IMediatorHandler _bus;
    public OrderEngineCommandHandler(
        ILogger<OrderEngineCommandHandler> logger,
        IOrderRepository repository, 
        IMediatorHandler bus, 
        INotificationHandler<DomainNotification> notifications) 
        : base(bus,notifications)  
    {
        _logger = logger;
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
        command.Order.OrderStatus = SharedX.Core.Enums.OrderStatus.Cancelled;
        command.Order.TransactTime = DateTime.UtcNow;

        var status = await _repository.UpdateOrderAsync(command.Order, cancellationToken);
        
        if (status)
            await _bus.RaiseEvent(new OrderTradeCancelEvent(command.Order));
        
        return status;
    }

    public async Task<bool> Handle(OrderCancelReplaceCommand command, CancellationToken cancellationToken)
    {
        if (!command.IsValid())
        {
            NotifyValidationErrors(command);
            return false;
        }

        var orderOld = await _repository.GetOrderByIdAsync(command.Order.OrderID, cancellationToken);
        
        if (IsOrderToReplace(orderOld, command.Order))
        {
            orderOld.OrderStatus = SharedX.Core.Enums.OrderStatus.Cancelled;
            orderOld.TransactTime = DateTime.UtcNow;
            var statusUpdate = await _repository.UpdateOrderAsync(orderOld, cancellationToken);
            
            if(statusUpdate)
                await _bus.RaiseEvent(new OrderTradeCancelEvent(orderOld));

            var idOrder = _repository.GetOrderIdAsync(cancellationToken).Result;
            command.Order.OrderID = idOrder;
            command.Order.LeavesQuantity = command.Order.Quantity;
            var statusCreate = await _repository.CreateOrdersAsync(command.Order, cancellationToken);
            
            if (statusCreate)
                await _bus.RaiseEvent(new OrderTradeNewEvent(command.Order));
        }
        else
        {
            var status = await _repository.UpdateOrderAsync(command.Order, cancellationToken);
            await _bus.RaiseEvent(new OrderTradeModifyEvent(command.Order));
        }
                
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
        command.Order.LeavesQuantity = command.Order.Quantity;

        await _repository.CreateOrdersAsync(command.Order, cancellationToken);
        await _bus.RaiseEvent(new OrderTradeNewEvent(command.Order));

        return true;
    }

    private bool IsOrderToReplace(OrderEngine orderOld, OrderEngine orderNew  )
    {
        return (orderOld.OrderType != orderNew.OrderType ||
            orderOld.Side != orderNew.Side ||
            orderOld.Symbol != orderNew.Symbol ||
            orderOld.LeavesQuantity < orderNew.Quantity ||
            orderOld.Price != orderNew.Price);
    }
}