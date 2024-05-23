using Microsoft.Extensions.Logging;
using SharedX.Core.Bus;
using SharedX.Core.Enums;
using MacthingX.Application.Interfaces;
using SharedX.Core.Matching.OrderEngine;
using MacthingX.Application.Commands.Match.OrderType;
using MatchingX.Core.Interfaces;

namespace MacthingX.Application.Services;
public class MatchMarket : IMatch
{
    protected readonly ITradeOrderService _tradeOrder;
    protected readonly IMediatorHandler Bus;

    public string Name => nameof(MatchMarket);

    public MatchMarket(ILogger<MatchMarket> logger, IMediatorHandler bus, ITradeOrderService tradeOrder) 
    {
        _tradeOrder = tradeOrder;
        Bus = bus;
    }
    public  void ReceiveOrder(OrderEngine order)
    {
        switch (order.Execution)
        {
            case Execution.ToCancel:
                this.CancelOrder(order);
                break;
            case Execution.ToModify:
                this.ModifyOrder(order);
                break;
            case Execution.ToOpen:
                _tradeOrder.AddOrder(order);
                break;
        }
    }

    public  bool CancelOrder(OrderEngine orderToCancel)
    {
        return _tradeOrder.CancelOrder(orderToCancel);
    }

    public  async Task<bool> MatchOrderAsync(OrderEngine order)
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

    public  bool ModifyOrder(OrderEngine order)
    {
        return _tradeOrder.ModifyOrder(order).Result;
    }
}