using MacthingX.Application.Commands.Match.OrderType;
using MacthingX.Application.Events;
using MatchingX.Application.Handlers;
using MatchingX.Core.Entities;
using MatchingX.Core.Interfaces;
using SharedX.Core.Enums;

namespace MatchingX.Application.Services;

public class MatchBase : IMatch
{
    public delegate void PriceChangedEventHandler(object sender, OrderPriceEventArgs args);
    protected readonly IMediatorHandler Bus;
    public virtual string Name => nameof(MatchBase);
    protected readonly CancellationTokenSource _cancellationTokenSource;
    public event PriceChangedEventHandler PriceChanged;
    protected readonly IMatchingRepository _repository;
    protected readonly IMatchingCache _cache;

    public MatchBase(IMediatorHandler bus, IMatchingRepository repository, IMatchingCache cache)
    {
        Bus = bus;
        _cancellationTokenSource = new CancellationTokenSource();
        _repository = repository;
        _cache = cache;
    }

    public async Task<bool> AddOrderAsync(MatchOrder order, CancellationToken cancellation)
    {
        var result = await _repository.UpsertOrderMatchingAsync(order, cancellation);
        if (result.IsSuccess)
        {
            await SetMatchingOrderCache(order, cancellation);
        }
        return result.IsSuccess;
    }

    public async Task<bool> CancelOrderAsync(MatchOrder order, CancellationToken cancellationToken)
    {
        var result = await _repository.RemoveOrdersMatchingAsync(new List<long>() { order.OrderID }, cancellationToken);

        if (result.IsSuccess)
        {
            await _cache.RemoveOrderMatchingAsync(order.Symbol, order.OrderID, order.Side);
        }
        return result.IsSuccess;
    }

    public async Task<bool> ModifyOrderAsync(MatchOrder order, CancellationToken cancellationToken)
    {
        var result = await _repository.UpsertOrderMatchingAsync(order, cancellationToken);
        if (result.IsSuccess)
        {
            await SetMatchingOrderCache(order, cancellationToken);
        }
        return result.IsSuccess;
    }

    public async Task<bool> ReceiveOrderAsync(MatchOrder order, CancellationToken cancellationToken) => order.Execution switch
    {
        Execution.ToCancel => await this.CancelOrderAsync(order, cancellationToken),
        Execution.ToModify => await this.ModifyOrderAsync(order, cancellationToken),
        Execution.ToOpen => await this.AddOrderAsync(order, cancellationToken),
    };

    public virtual async Task<bool> MatchOrderAsync(MatchOrder order, CancellationToken cancellationToken)
    {
        var match = Bus.SendMatchCommand(new MatchingStopLimitCommand(order)).Result;

        if (match.PrincipalOrder.OrderStatus is OrderStatus.Filled or OrderStatus.PartiallyFilled)
        {
            var @event = new OrderTradedEvent(order);
            await Bus.Publish(@event);
        }
        else if (match.PrincipalOrder.OrderStatus is OrderStatus.Cancelled)
        {
            var @event = new OrderCanceledEvent(order);
            await Bus.Publish(@event);
        }
        return true;
    }

    private async Task SetMatchingOrderCache(MatchOrder order, CancellationToken cancellation)
    {
        var orderToExecute = new MatchingEngine();

        orderToExecute.PrincipalOrder = order;

        await _cache.UpsertOrderMatchingAsync(orderToExecute, cancellation);
    }

}
