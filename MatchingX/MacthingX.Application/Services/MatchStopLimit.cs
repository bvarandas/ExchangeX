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

    public string Name => nameof(MatchStopLimit);

    public MatchStopLimit(ILogger<MatchStopLimit> logger,
        IMediatorHandler bus,
        IOrderStopCache orderStopCache,
        IMatchingRepository repository) : base(bus, repository)
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

        var ordersStop = order.Side == SideTrade.Sell ?
            await _orderStopCache.GetBuyOrderBySymbolAsync(order.Symbol) :
            await _orderStopCache.GetSellOrderBySymbolAsync(order.Symbol);

        if (ordersStop.IsSuccess)
        {
            foreach (var ordersStopPrice in ordersStop.Value)
            {
                if (DicOrdersToCancel.TryGetValue(order.OrderID, out MatchingEngine orderFound))
                    DicOrdersToCancel.Remove(order.OrderID, out MatchingEngine orderRemoved);

                switch ((order.OrderType, order.Side))
                {
                    case (OrderType.Stop, SideTrade.Sell) when order.StopPrice >= price:
                        await _orderStopCache.DeleteSellOrderAsync(order.Symbol, order.OrderID);

                        await this.MatchOrderAsync(order, _cancellationTokenSource.Token);

                        break;
                    case (OrderType.Stop, SideTrade.Buy) when order.StopPrice <= price:
                        await _orderStopCache.DeleteBuyOrderAsync(order.Symbol, order.OrderID);

                        await this.MatchOrderAsync(order, _cancellationTokenSource.Token);
                        break;
                    default:
                        break;
                }
            }
        }
    }




}