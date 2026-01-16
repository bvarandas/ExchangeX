using MacthingX.Application.Events;
using MacthingX.Application.Extensions;
using MatchingX.Application.Handlers;
using MatchingX.Application.Services;
using MatchingX.Core.Entities;
using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SharedX.Core.Enums;
using SharedX.Core.Matching.OrderEngine;
namespace MacthingX.Application.Services;
public class MatchingReceiver : IMatchingReceiver
{
    private readonly ILogger<MatchingReceiver> _logger;
    private readonly IMediatorHandler _mediator;
    private readonly IMatchingCache _matchingCache;
    private readonly IMatchContextStrategy _contextStrategy;

    public MatchingReceiver(ILogger<MatchingReceiver> logger,
        IMatchingCache matchingCache,
        IMatchContextStrategy contextoStrategy,
        IMediatorHandler mediator
        )
    {
        _mediator = mediator;
        _logger = logger;
        _matchingCache = matchingCache;
        _contextStrategy = contextoStrategy;
    }
    public async Task ReceiveOrder(OrderEngine order, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Chegou Order do tipo {order.OrderType.ToString()}");

        var matchOrder = order.ToMatching();

        switch (order.Execution)
        {
            case Execution.ToOpen:
                this.SetStrategy(matchOrder.PrincipalOrder);
                _contextStrategy.ReceivedOrder(matchOrder, cancellationToken);
                await _mediator.Publish(new OrderOpenedEvent(matchOrder.PrincipalOrder));
                break;
            case Execution.ToCancel:
                this.SetStrategy(matchOrder.PrincipalOrder);
                _contextStrategy.ReceivedOrder(matchOrder, cancellationToken);
                await _mediator.Publish(new OrderCanceledEvent(matchOrder.PrincipalOrder));
                break;
            case Execution.ToModify:
                this.SetStrategy(matchOrder.PrincipalOrder);
                _contextStrategy.ReceivedOrder(matchOrder, cancellationToken);
                await _mediator.Publish(new OrderModifiedEvent(matchOrder.PrincipalOrder));
                break;
        }
    }

    private bool SetStrategy(MatchOrder order) => order.OrderType switch
    {
        OrderType.Limit => _contextStrategy.SetStrategy(nameof(MatchLimit)),
        OrderType.Market => _contextStrategy.SetStrategy(nameof(MatchMarket)),
        OrderType.StopLimit => _contextStrategy.SetStrategy(nameof(MatchStopLimit)),
        OrderType.Stop => _contextStrategy.SetStrategy(nameof(MatchStop)),
        _ => false
    };
}