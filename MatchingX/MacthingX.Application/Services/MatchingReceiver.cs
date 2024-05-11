using MacthingX.Application.Commands;
using MacthingX.Application.Events;
using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SharedX.Core.Bus;
using SharedX.Core.Enums;
using SharedX.Core.Matching.OrderEngine;

namespace MacthingX.Application.Services;
public class MatchingReceiver : IMatchingReceiver
{
    private readonly ILogger<MatchingReceiver> _logger;
    private readonly MatchContextStrategy _contextStrategy;
    private readonly IMediatorHandler _mediator;
    private readonly IMatchingCache _matchingCache;
    public MatchingReceiver(ILogger<MatchingReceiver> logger,
        IMatchingCache matchingCache)
    {
        _logger = logger;
        _matchingCache = matchingCache;
        _contextStrategy = new MatchContextStrategy();
    }
    public void ReceiveOrder(OrderEngine order) 
    {
        _logger.LogInformation($"Chegou Order do tipo {order.OrderType.ToString()}");
        switch(order.Execution)
        {
            case Execution.ToOpen:
                _mediator.SendCommand(new OrderOpenedCommand(order, _matchingCache));
                break;
            case Execution.ToCancel:
                _mediator.SendCommand(new OrderCancelCommand(order, _matchingCache));
                break;
            case Execution.ToCancelReplace:
                _mediator.SendCommand(new OrderCancelReplaceCommand(order, _matchingCache));
                break;
        }
    }
}