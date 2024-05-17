using Microsoft.Extensions.Logging;
using SharedX.Core.Bus;
using SharedX.Core.Enums;
using MacthingX.Application.Interfaces;
using SharedX.Core.Matching.OrderEngine;
using MatchingX.Core.Interfaces;
using MatchingX.Infra.Repositories;
using MacthingX.Application.Commands.Match;
using MassTransit;

namespace MacthingX.Application.Services;
public class MatchMarket : IMatchMarket, IMatch
{
    protected readonly ITradeOrderService _tradeOrder;
    protected readonly IMediatorHandler Bus;

    private readonly IMatchingRepository _matchingRepository;
    public MatchMarket(ILogger<MatchMarket> logger, 
        IMediatorHandler bus, 
        ITradeOrderService tradeOrder) 
    {
        _tradeOrder = tradeOrder;
        Bus = bus;
    }
    public void ReceiveOrder(OrderEngine order)
    {
        this.AddOrder(order);
    }
    public bool AddOrder(OrderEngine order)
    {
        return _tradeOrder.AddOrder(order);
    }
    public bool CancelOrder(OrderEngine orderToCancel)
    {
        return _tradeOrder.CancelOrder(orderToCancel);
    }

    public bool ReplaceOrder(OrderEngine orderToReplace)
    {
        return _tradeOrder.ReplaceOrder(orderToReplace);
    }

    public async Task<bool> MatchOrderAsync(OrderEngine order)
    {
        bool cancelled = false;
        var result = Bus.SendMatchCommand(new MatchingMarketCommand(order)).Result;

        if (result.Item1 is OrderStatus.Filled or OrderStatus.PartiallyFilled)
        {
            _tradeOrder.CreateReports(order, result.Item2);
            await _tradeOrder.RemoveTradedOrdersAsync(result.Item2);
        }
        else
        {
            if (order.TimeInForce == TimeInForce.FOK)
                cancelled = _tradeOrder.RemoveCancelledOrdersAsync(order).Result;
        }
        return true;
    }

    public bool ModifyOrder(OrderEngine order)
    {
        throw new NotImplementedException();
    }
}