using MatchingX.Application.Handlers;
using MatchingX.Application.Services;
using MatchingX.Core.Entities;
using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Logging;

using SharedX.Core.Enums;
using System.Collections.Concurrent;
namespace MacthingX.Application.Services;
public sealed class MatchStopLimit : MatchBase
{
    protected readonly IOrderStopCache _orderStopCache;
    private readonly ConcurrentDictionary<long, MatchingEngine> DicOrdersToCancel;
    protected readonly IMediatorHandler Bus;

    public override string Name => nameof(MatchStopLimit);

    public MatchStopLimit(ILogger<MatchStopLimit> logger,
        IMediatorHandler bus,
        IOrderStopCache orderStopCache,
        IMatchingRepository repository,
        IMatchingCache cache) : base(bus, repository, cache)
    {
        DicOrdersToCancel = new ConcurrentDictionary<long, MatchingEngine>();

        _orderStopCache = orderStopCache;
        Bus = bus;
        PriceChanged += TradeOrder_PriceChanged;

    }


    private async void TradeOrder_PriceChanged(object sender, Events.OrderPriceEventArgs args)
    {
        var order = args.Order;
        decimal price = order.Price;

        var ordersStop = await _orderStopCache.GetOrderBySymbolAsync(order.Symbol, order.Side);

        if (ordersStop.IsSuccess)
        {
            foreach (var ordersStopPrice in ordersStop.Value)
            {
                if (DicOrdersToCancel.TryGetValue(order.OrderID, out MatchingEngine orderFound))
                    DicOrdersToCancel.Remove(order.OrderID, out MatchingEngine orderRemoved);

                switch ((order.OrderType, order.Side))
                {
                    case (OrderType.Stop, SideTrade.Sell) or
                         (OrderType.StopLimit, SideTrade.Sell) when
                         order.StopPrice >= price:
                        {
                            await _orderStopCache.DeleteOrderAsync(order.Symbol, order.OrderID, order.Side);
                            await this.MatchOrderAsync(order, _cancellationTokenSource.Token);
                        }
                        break;
                    case (OrderType.Stop, SideTrade.Buy) or
                         (OrderType.StopLimit, SideTrade.Buy) when
                         order.StopPrice <= price:
                        {
                            await _orderStopCache.DeleteOrderAsync(order.Symbol, order.OrderID, order.Side);
                            await this.MatchOrderAsync(order, _cancellationTokenSource.Token);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }




}