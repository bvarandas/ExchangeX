using MacthingX.Application.Events;
using MediatR;
using SharedX.Core.Bus;
using SharedX.Core.Interfaces;
using SharedX.Core.Enums;
using MatchingX.Application.Commands;
using MatchingX.Core.Notifications;
using SharedX.Core.Matching.OrderEngine;
using MacthingX.Application.Commands.Order;

namespace MacthingX.Application.Commands;

public class OrderCommandHandler :
    CommandHandler,
    IRequestHandler<OrderOpenedCommand, bool>,
    IRequestHandler<OrderCancelCommand, bool>,
    IRequestHandler<OrderCancelReplaceCommand, bool>,
    IRequestHandler<OrderPartiallyFilledCommand, bool>,
    IRequestHandler<OrderFilledCommand, bool>
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

    public async Task<bool> Handle(OrderOpenedCommand command, CancellationToken cancellationToken)
    {
        command.Order.LastQuantity = command.Order.Quantity;

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
        //O pedido OrderCancelReplaceRequest suporta a modificação de um pedido
        //existente ou a substituição de um pedido existente,
        //cancelando o pedido existente e inserindo um novo pedido com base nos parâmetros da solicitação
        //Se a solicitação for apenas para reduzir as QtyLeaves de um pedido
        //(ordem da solicitação <QtyLeaves da ordem);
        //Outros campos podem apresentar os mesmos valores que a ordem original ou serem
        //omitidos da solicitação, o sistema modifica a ordem existente,
        
        //Verificando se a ordem é para modificação ou substituição
        var orderToCompare = await _repository.GetOrderByIdAsync(command.Order.OrderID, cancellationToken);

        bool mustReplace = (orderToCompare.Price != command.Order.Price) || 
             (orderToCompare.OrderType != command.Order.OrderType) ||
             (orderToCompare.Side != command.Order.Side) ||
             (orderToCompare.Symbol != command.Order.Symbol);

        var orderDetail = new OrderEngineDetail();
        orderDetail.OrderID = command.Order.OrderID;
        orderDetail.LastPrice = command.Order.LastPrice;
        orderDetail.LastQuantity = command.Order.LastQuantity;
        orderDetail.CumQty = command.Order.Quantity - command.Order.LastQuantity;
        orderDetail.Symbol = command.Order.Symbol;
        orderDetail.LeavesQuantity = command.Order.LeavesQuantity;
        orderDetail.TransactTime = DateTime.Now;

        if (command.Order.OrderDetails is null)
            command.Order.OrderDetails = new List<OrderEngineDetail>();

        if (!mustReplace && (orderToCompare.LeavesQuantity > orderToCompare.Quantity))
        {

            await _repository.UpdateOrderAsync(command.Order, cancellationToken);
        }

        if(mustReplace)
        {
            //Cancelando a ordem
            command.Order.OrderStatus   = OrderStatus.Cancelled;
            
            command.Order.OrderDetails.Add(orderDetail);
            await _repository.UpdateOrderAsync(command.Order, cancellationToken);
            await _bus.RaiseEvent(new OrderCanceledEvent(command.Order));

            //Inserindo a nova ordem
            var newOrderId = _repository.GetOrderIdAsync(cancellationToken);
            command.Order.OrderID = newOrderId.Result;
            command.Order.OrderStatus = OrderStatus.New;
            
            await _repository.UpdateOrderAsync(command.Order, cancellationToken);
            await _bus.RaiseEvent(new OrderOpenedEvent(command.Order));
        }

        return await Task.FromResult(true);
    }

    public async Task<bool> Handle(OrderPartiallyFilledCommand command, CancellationToken cancellationToken)
    {
        var order = command.Order;
        var orderDetail = new OrderEngineDetail();
        var lastOrderDetail = new OrderEngineDetail();

        var orderData = await _repository.GetOrderByIdAsync(command.Order.OrderID, cancellationToken);
        if (orderData.OrderDetails.Any())
        {
            lastOrderDetail = orderData.OrderDetails.LastOrDefault();
        }

        orderDetail.Symbol = order.Symbol;
        orderDetail.OrderID = order.OrderID;
        orderDetail.LastPrice = order.LastPrice;
        orderDetail.CumQty = lastOrderDetail.CumQty + order.LastQuantity;
        orderDetail.LeavesQuantity = order.Quantity - orderDetail.CumQty;
        orderDetail.LastQuantity = order.LastQuantity;

        orderData.OrderDetails.Add(orderDetail);

        await _repository.UpdateOrderAsync(orderData, cancellationToken);

        await _bus.RaiseEvent(new OrderTradedEvent(command.Order));

        return await Task.FromResult(true);
    }

    public async Task<bool> Handle(OrderFilledCommand command, CancellationToken cancellationToken)
    {
        var order = command.Order;
        var orderDetail = new OrderEngineDetail();
        
        var orderData = await _repository.GetOrderByIdAsync(command.Order.OrderID, cancellationToken);
        
        orderDetail.Symbol = order.Symbol;
        orderDetail.OrderID = order.OrderID;
        orderDetail.LastPrice = order.LastPrice;
        orderDetail.CumQty = order.Quantity;
        orderDetail.LeavesQuantity = 0;
        orderDetail.LastQuantity = order.LastQuantity;
        orderDetail.TransactTime = order.TransactTime;

        orderData.OrderDetails.Add(orderDetail);

        await _repository.UpdateOrderAsync(orderData, cancellationToken);

        await _bus.RaiseEvent(new OrderTradedEvent(command.Order));

        return await Task.FromResult(true);
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
}
