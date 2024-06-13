using MacthingX.Application.Events;
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
    public void ReceiveOrder(OrderEngine order) 
    {
        _logger.LogInformation($"Chegou Order do tipo {order.OrderType.ToString()}");
        switch(order.Execution)
        {
            case Execution.ToOpen:
                this.SetStrategy(ref order);
                _contextStrategy.ReceivedOrder(order);
                _mediator.Publish(new OrderOpenedEvent(order));
                break;
            case Execution.ToCancel:
                this.SetStrategy(ref order);
                _contextStrategy.ReceivedOrder(order);
                _mediator.Publish(new OrderCanceledEvent(order));
                break;
            case Execution.ToModify:
                this.SetStrategy(ref order);
                _contextStrategy.ReceivedOrder(order);
                _mediator.Publish(new OrderModifiedEvent(order));
                break;
        }
    }
    
    private void SetStrategy(ref OrderEngine order)
    {
        switch (order.OrderType)
        {
            case OrderType.Limit:
                _contextStrategy.SetStrategy(nameof(MatchLimit));
                break;
            case OrderType.Market:
                _contextStrategy.SetStrategy(nameof(MatchMarket));
                break;
            case OrderType.StopLimit:
                _contextStrategy.SetStrategy(nameof(MatchStopLimit));
                break;
            case OrderType.Stop:
                _contextStrategy.SetStrategy(nameof(MatchStop));
                break;
        }
    }
    //public void ReceiveSecurity(SecurityEngine security)
    //{
    //    _contextStrategy.ReceivedSecurity(security);
    //}
}