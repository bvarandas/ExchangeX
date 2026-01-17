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
    private readonly IMediatorHandler _mediator;
    public ReceiverOrder(ILogger<ReceiverOrder> logger, IMediatorHandler mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public async Task ReceiveEngine(OrderEngine message, CancellationToken cancellationToken)
    {
        var command = GetCommand(message);
        try
        {
            await _mediator.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }
    }

    private OrderEngineCommand GetCommand(OrderEngine order) => order.Execution switch
    {
        Execution.ToCancel => new OrderCancelCommand(order),
        Execution.ToCancelReplace => new OrderCancelReplaceCommand(order),
        Execution.ToOpen => new OrderOpenedCommand(order),
    };
}