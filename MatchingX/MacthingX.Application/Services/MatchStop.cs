using MacthingX.Application.Events;
using MatchingX.Application.Handlers;
using MatchingX.Core.Entities;
using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SharedX.Core.Enums;
using System.Collections.Concurrent;
namespace MatchingX.Application.Services;
public sealed class MatchStop : MatchBase
{
    private readonly ConcurrentDictionary<long, MatchOrder> DicOrdersToCancel;
    protected readonly IOrderStopCache _orderStopCache;
    protected readonly IMediatorHandler Bus;

    public override string Name => nameof(MatchStop);

    public MatchStop(ILogger<MatchStop> logger, IMediatorHandler bus, IOrderStopCache orderStopCache, IMatchingRepository repository) : base(bus, repository)
    {
        DicOrdersToCancel = new ConcurrentDictionary<long, MatchOrder>();

        Bus = bus;
        _orderStopCache = orderStopCache;
        PriceChanged += TradeOrder_PriceChanged;
    }

    private async void TradeOrder_PriceChanged(object sender, OrderPriceEventArgs args)
    {
        var order = args.Order;
        decimal price = order.Price;

        var ordersStop = (order.Side == SideTrade.Sell) ?
            await _orderStopCache.GetBuyOrderBySymbolAsync(order.Symbol) :
            await _orderStopCache.GetSellOrderBySymbolAsync(order.Symbol);

        if (!ordersStop.IsSuccess)
            return;

        foreach (var ordersStopPrice in ordersStop.Value)
        {
            if (DicOrdersToCancel.TryGetValue(order.OrderID, out MatchOrder orderFound))
                DicOrdersToCancel.Remove(order.OrderID, out MatchOrder orderRemoved);

            switch ((order.OrderType, order.Side))
            {
                case (OrderType.Stop, SideTrade.Sell) when order.StopPrice >= price:
                    //await _orderStopCache.DeleteOrderAsync(order.Symbol, order.OrderID);

                    await this.MatchOrderAsync(order, _cancellationTokenSource.Token);

                    break;
                case (OrderType.Stop, SideTrade.Buy) when order.StopPrice <= price:
                    //await _orderStopCache.DeleteOrderAsync(order.Symbol, order.OrderID);

                    await this.MatchOrderAsync(order, _cancellationTokenSource.Token);
                    break;
                default:
                    break;
            }
        }

    }


}