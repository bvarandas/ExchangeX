using MarketDataX.Application.Commands;
using MediatR;
using SharedX.Core.Bus;
using SharedX.Core.Interfaces;
using SharedX.Core.Extensions;
using OrderEngineX.Application.Events;
using SharedX.Core.Matching;

namespace OrderEngineX.Application.Commands;
public class OrderTradeCommandHandler :
    IRequestHandler<OrderTradeCancelCommand, bool>,
    IRequestHandler<OrderTradeModifyCommand, bool>,
    IRequestHandler<OrderTradeNewCommand, bool>
{
    private readonly IOrderRepository _repository;
    private readonly IMediatorHandler _bus;
    public OrderTradeCommandHandler(IOrderRepository repository, IMediatorHandler bus)
    {
        _repository = repository;
        _bus = bus;
    }
    public async Task<bool> Handle(OrderTradeCancelCommand command, CancellationToken cancellationToken)
    {
        command.Order.OrderStatus = SharedX.Core.Enums.OrderStatus.Cancelled;

        await _repository.UpdateOrderAsync(command.Order.ToOrder(), cancellationToken);
        await _bus.RaiseEvent(new OrderTradeCancelEvent(command.Order));
        return true;
    }

    public async Task<bool> Handle(OrderTradeModifyCommand command, CancellationToken cancellationToken)
    {
        await _bus.RaiseEvent(new OrderTradeModifyEvent(command.Order));
        return true;
    }

    public async Task<bool> Handle(OrderTradeNewCommand command, CancellationToken cancellationToken)
    {
        var idOrder = _repository.GetOrderIdAsync(cancellationToken).Result;
        command.Order.OrderID = idOrder;

        await _repository.CreateOrdersAsync(command.Order.ToOrder(), cancellationToken);
        await _bus.RaiseEvent(new OrderTradeNewEvent(command.Order));

        return true;
    }
}