using MacthingX.Application.Events;
using MacthingX.Application.Interfaces;
using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SharedX.Core.Bus;
using SharedX.Core.Entities;
using SharedX.Core.Enums;
using SharedX.Core.Matching.OrderEngine;
namespace MacthingX.Application.Services;
public class MatchingReceiver : IMatchingReceiver
{
    private readonly ILogger<MatchingReceiver> _logger;
    private readonly IMediatorHandler _mediator;
    private readonly IMatchingCache _matchingCache;
    private readonly IMatchContextStrategy _contextStrategy;
    private readonly IMatchMarket _match;
    
    public MatchingReceiver(ILogger<MatchingReceiver> logger,
        IMatchingCache matchingCache,
        IMatchContextStrategy contextoStrategy,
        IMatchMarket match)
    {
        _match = match;
        _logger = logger;
        _matchingCache = matchingCache;
        _contextStrategy = contextoStrategy;
    }
    public void ReceiveOrder(OrderEngine order) 
    {
        _logger.LogInformation($"Chegou Order do tipo {order.OrderType.ToString()}");
        switch(order.Execution)
        {
            case Execution.ToOpen:
                this.SetStrategy(ref order);
                _contextStrategy.ReceivedOrder(order);
                _mediator.RaiseEvent(new OrderOpenedEvent(order));
                break;
            case Execution.ToCancel:
                this.SetStrategy(ref order);
                _contextStrategy.ReceivedOrder(order);
                _mediator.RaiseEvent(new OrderCanceledEvent(order));
                break;
            case Execution.ToModify:
                this.SetStrategy(ref order);
                _contextStrategy.ReceivedOrder(order);
                _mediator.RaiseEvent(new OrderModifiedEvent(order));
                break;
        }
    }
    
    private void SetStrategy(ref OrderEngine order)
    {
        switch (order.OrderType)
        {
            case OrderType.Limit:
                _contextStrategy.SetStrategy((MatchLimit)_match);
                break;
            case OrderType.Market:
                _contextStrategy.SetStrategy((MatchMarket)_match);
                break;
            case OrderType.StopLimit:
                _contextStrategy.SetStrategy((MatchStopLimit)_match);
                break;
            case OrderType.Stop:
                _contextStrategy.SetStrategy((MatchStop)_match);
                break;
        }
    }
    public void ReceiveSecurity(SecurityEngine security)
    {

    }
}