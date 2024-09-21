using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderEngineX.Application.Commands.Order;
using OrderEngineX.Application.Events;
using OrderEngineX.Core.Interfaces;
using OrderEngineX.Core.Notifications;
using SharedX.Core.Bus;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;

namespace OrderEngineX.Application.Commands;
public class OrderEngineCommandHandler : CommandHandler,
    IRequestHandler<OrderCancelCommand, Result>,
    IRequestHandler<OrderCancelReplaceCommand, Result>,
    IRequestHandler<OrderOpenedCommand, Result>
{
    private readonly ILogger<OrderEngineCommandHandler> _logger;
    private readonly IOrderEngineRepository _repository;
    private readonly IMediatorHandler _bus;
    private readonly IPublisherEngine<OrderEngine> _publisherEngine;
    public OrderEngineCommandHandler(
        ILogger<OrderEngineCommandHandler> logger,
        IOrderEngineRepository repository,
        IMediatorHandler bus,
        INotificationHandler<DomainNotification> notifications,
        IPublisherEngine<OrderEngine> publisherEngine)
        : base(bus, notifications)
    {
        _logger = logger;
        _repository = repository;
        _bus = bus;
        _publisherEngine = publisherEngine;
    }

    public async Task<Result> Handle(OrderCancelCommand command, CancellationToken cancellationToken)
    {
        if (!command.IsValid())
        {
            NotifyValidationErrors(command);
            return Result.Fail(new Error(""));
        }
        command.Order.OrderStatus = SharedX.Core.Enums.OrderStatus.Cancelled;
        command.Order.TransactTime = DateTime.UtcNow;

        var status = await _repository.UpsertOrdersAsync(command.Order, cancellationToken);

        if (status.IsSuccess)
        {
            _publisherEngine.PublishEngine(command.Order);
            await _bus.Publish(new OrderEngineCancelEvent(command.Order));
            return Result.Ok();
        }
        return status;
    }

    public async Task<Result> Handle(OrderCancelReplaceCommand command, CancellationToken cancellationToken)
    {
        if (!command.IsValid())
        {
            NotifyValidationErrors(command);
            return Result.Fail(new Error(""));
        }

        var orderOld = await _repository.GetOrderByIdAsync(command.Order.OrderID, cancellationToken);
        if (orderOld.IsSuccess)
            if (IsOrderToReplace(orderOld.Value, command.Order))
            {
                orderOld.Value.OrderStatus = SharedX.Core.Enums.OrderStatus.Cancelled;
                orderOld.Value.TransactTime = DateTime.UtcNow;
                var statusUpdate = await _repository.UpsertOrdersAsync(orderOld.Value, cancellationToken);

                if (statusUpdate.IsSuccess)
                    await _bus.Publish(new OrderEngineCancelEvent(orderOld.Value));

                var idOrder = _repository.GetOrderIdAsync(cancellationToken).Result;
                command.Order.OrderID = idOrder.Value;
                command.Order.LeavesQuantity = command.Order.Quantity;
                var statusCreate = await _repository.CreateOrdersAsync(command.Order, cancellationToken);

                if (statusCreate.IsSuccess)
                {
                    _publisherEngine.PublishEngine(command.Order);
                    await _bus.Publish(new OrderEngineNewEvent(command.Order));
                }
            }
            else
            {
                var status = await _repository.UpsertOrdersAsync(command.Order, cancellationToken);
                _publisherEngine.PublishEngine(command.Order);
                await _bus.Publish(new OrderTradeModifyEvent(command.Order));
            }

        return Result.Ok();
    }

    public async Task<Result> Handle(OrderOpenedCommand command, CancellationToken cancellationToken)
    {
        if (!command.IsValid())
        {
            NotifyValidationErrors(command);
            return Result.Fail(new Error(""));
        }

        var idOrder = _repository.GetOrderIdAsync(cancellationToken).Result;
        command.Order.OrderID = idOrder.Value;
        command.Order.LeavesQuantity = command.Order.Quantity;

        var result = await _repository.CreateOrdersAsync(command.Order, cancellationToken);
        if (result.IsSuccess)
        {
            _publisherEngine.PublishEngine(command.Order);
            await _bus.Publish(new OrderEngineNewEvent(command.Order));
        }


        return Result.Ok();
    }

    private bool IsOrderToReplace(OrderEngine orderOld, OrderEngine orderNew)
    {
        return (orderOld.OrderType != orderNew.OrderType ||
            orderOld.Side != orderNew.Side ||
            orderOld.Symbol != orderNew.Symbol ||
            orderOld.LeavesQuantity < orderNew.Quantity ||
            orderOld.Price != orderNew.Price);
    }
}