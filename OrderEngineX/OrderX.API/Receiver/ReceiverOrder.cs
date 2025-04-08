using MatchingX.Core.Interfaces;
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

    public async Task ReceiveEngine(OrderEngine message, CancellationToken cancellationToken)
    {
        var command = GetCommand(message);
        await _mediator.Send(command);
    }

    private OrderEngineCommand GetCommand(OrderEngine order) => order.Execution switch
    {
        Execution.ToCancel => new OrderCancelCommand(order, _cache),
        Execution.ToCancelReplace => new OrderCancelReplaceCommand(order, _cache),
        Execution.ToOpen => new OrderOpenedCommand(order, _cache),
    };
}