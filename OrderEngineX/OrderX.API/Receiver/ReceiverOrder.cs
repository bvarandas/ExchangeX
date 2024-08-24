using OrderEngineX.Application.Commands;
using OrderEngineX.Application.Commands.Order;
using SharedX.Core.Bus;
using SharedX.Core.Enums;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;

namespace OrderEngineX.API.Receiver;

public class ReceiverOrder : IReceiverEngine<OrderEngine>
{
    private readonly ILogger<ReceiverOrder> _logger;
    private readonly IBookOfferCache _cache;
    private readonly IMediatorHandler _mediator;

    public ReceiverOrder(ILogger<ReceiverOrder> logger, IBookOfferCache cache, IMediatorHandler mediator)
    {
        _logger = logger;
        _cache = cache;
        _mediator = mediator;
    }

    public void ReceiveEngine(OrderEngine message, CancellationToken cancellationToken)
    {
        SendOrderCommand(message);
    }

    private bool SendOrderCommand(OrderEngine order)
    {
        OrderEngineCommand command = null!;
        switch (order.Execution)
        {
            case Execution.ToCancel:
                command = new OrderCancelCommand(order, _cache);
                break;
            case Execution.ToCancelReplace:
                command = new OrderCancelReplaceCommand(order, _cache);
                break;
            case Execution.ToOpen:
                command = new OrderOpenedCommand(order, _cache);
                break;
        }

        var result = _mediator.Send(command);
        return result.Result.IsSuccess;
    }
}
