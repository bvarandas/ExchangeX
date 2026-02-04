using MacthingX.Application.Commands.Match;
using MacthingX.Application.Commands.Match.OrderType;
using MatchingX.Application.Handlers;
using MatchingX.Core.Entities;
using MatchingX.Core.Interfaces;
using MatchingX.Core.Notifications;
using MatchingX.Core.Repositories;
using Medallion.Threading;
using MediatR;
using SharedX.Core.Enums;

namespace MacthingX.Application.Handlers;
public class MatchingCommandHandler :
    CommandHandler,
    IRequestHandler<MatchingLimitCommand, MatchingEngine>,
    IRequestHandler<MatchingMarketCommand, MatchingEngine>,
    IRequestHandler<MatchingStopLimitCommand, MatchingEngine>,
    IRequestHandler<MatchingStopCommand, MatchingEngine>
{
    private readonly IMatchingRepository _matchRepository;
    private readonly IExecutedTradeRepository _tradeRepository;
    private readonly IMediatorHandler _bus;
    private readonly IDistributedLockProvider _distributedLockProvider;
    private static IMatchingCache _matchingCache;
    private static IBookOfferCache _bookOfferCache;

    public MatchingCommandHandler(IMatchingRepository repository,
        IExecutedTradeRepository tradeRepository,
        IMediatorHandler bus,
        INotificationHandler<DomainNotification> notifications,
        IDistributedLockProvider distributedLockProvider,
        IMatchingCache matchingCache,
        IBookOfferCache bookOfferCache)
        : base(bus, notifications, matchingCache)
    {
        _matchingCache = matchingCache;
        _tradeRepository = tradeRepository;
        _matchRepository = repository;
        _bus = bus;
        _distributedLockProvider = distributedLockProvider;
        _bookOfferCache = bookOfferCache;
    }

    private static async Task<MatchingEngine> MachtingMaking(MatchingEngineCommand command, CancellationToken stoppingToken)
    {
        var orderToMatch = command.Order.Side == SideTrade.Buy ?
                await _bookOfferCache.GetOrderBySymbolAsync(command.Order.Symbol, SideTrade.Sell) :
                await _bookOfferCache.GetOrderBySymbolAsync(command.Order.Symbol, SideTrade.Buy);

        var orderToExecute = new MatchingEngine();

        Dictionary<long, MatchOrder> dicOrderToMatch;

        if (command.Order.Side == SideTrade.Buy)
        {
            dicOrderToMatch = orderToMatch.Value
                                        .Where(i =>
                                        i.Value.LastQuantity <= command.Order.LastQuantity &&
                                        i.Value.Price <= command.Order.Price)
                                        .OrderByDescending(i => i.Value.Price)
                                        .ToDictionary(i => i.Key, i => i.Value);
        }
        else
        {

            dicOrderToMatch = orderToMatch.Value
                                        .Where(i =>
                                        i.Value.LastQuantity <= command.Order.LastQuantity &&
                                        i.Value.Price <= command.Order.Price)
                                        .OrderBy(i => i.Value.Price)
                                        .ToDictionary(i => i.Key, i => i.Value);
        }
        if (command.Order.TimeInForce != TimeInForce.FOK)
        {
            orderToExecute = await Execute(command, dicOrderToMatch, stoppingToken);
        }
        else if (command.Order.TimeInForce == TimeInForce.FOK)
        {
            orderToExecute = await ExecuteFok(command, dicOrderToMatch, stoppingToken);
        }

        return orderToExecute;
    }

    public async Task<MatchingEngine> Handle(MatchingLimitCommand command, CancellationToken cancellationToken)
    {
        string nameLock = $"{command.Order.Symbol}_{command.Order.OrderType}";
        await using (await _distributedLockProvider.TryAcquireLockAsync(nameLock, TimeSpan.FromSeconds(3), cancellationToken))
        {
            var result = await MachtingMaking(command, cancellationToken);

            return result;
        }

    }

    public async Task<MatchingEngine> Handle(MatchingMarketCommand command, CancellationToken cancellationToken)
    {
        string nameLock = $"{command.Order.Symbol}_{command.Order.OrderType}";

        await using (await _distributedLockProvider.TryAcquireLockAsync(nameLock, TimeSpan.FromSeconds(3), cancellationToken))
        {
            var result = await MachtingMaking(command, cancellationToken);

            return result;
        }
    }

    public async Task<MatchingEngine> Handle(MatchingStopLimitCommand command, CancellationToken cancellationToken)
    {
        string nameLock = $"{command.Order.Symbol}_{command.Order.OrderType}";

        await using (await _distributedLockProvider.TryAcquireLockAsync(nameLock, TimeSpan.FromSeconds(3), cancellationToken))
        {
            var result = await MachtingMaking(command, cancellationToken);

            return result;
        }
    }

    public async Task<MatchingEngine> Handle(MatchingStopCommand command, CancellationToken cancellationToken)
    {
        string nameLock = $"{command.Order.Symbol}_{command.Order.OrderType}";

        await using (await _distributedLockProvider.TryAcquireLockAsync(nameLock, TimeSpan.FromSeconds(3), cancellationToken))
        {
            var result = await MachtingMaking(command, cancellationToken);

            return result;
        }
    }

    private static async Task<MatchingEngine> Execute(MatchingEngineCommand command,
        Dictionary<long, MatchOrder> dicOrderToMatch, CancellationToken cancellation)
    {
        var orderToExecute = new MatchingEngine();

        decimal quantityCollected = 0.0M;

        foreach (var order in dicOrderToMatch.Values)
        {
            if (command.Order.LeavesQuantity > (quantityCollected + order.LeavesQuantity))
            {
                quantityCollected += order.LeavesQuantity;

                if (command.Order.LeavesQuantity == quantityCollected)
                    command.Order.OrderStatus = OrderStatus.Filled;
                else
                    command.Order.OrderStatus = OrderStatus.PartiallyFilled;

                orderToExecute.PrincipalOrder = command.Order;
                orderToExecute.PartOrders.Add(order.OrderID, order);
            }
        }

        // se deu PartiallyFilled então atualizar do redis
        if (command.Order.OrderStatus == OrderStatus.PartiallyFilled ||
            command.Order.OrderStatus == OrderStatus.Filled)
        {
            foreach (var order in orderToExecute.PartOrders.Values)
            {
                //command.Order.LeavesQuantity -= order.LeavesQuantity;
                order.LeavesQuantity = 0;
                order.OrderStatus = OrderStatus.Filled;
                order.TransactTime = DateTime.Now;
                orderToExecute.PrincipalOrder.LastPrice = order.Price;
                order.LastPrice = order.Price;
            }
        }

        await _matchingCache.UpsertOrderMatchingAsync(orderToExecute, cancellation);

        return orderToExecute;
    }

    private static async Task<MatchingEngine> ExecuteFok(MatchingEngineCommand command,
        Dictionary<long, MatchOrder> dicOrderToMatch, CancellationToken cancellation)
    {
        var orderToExecute = new MatchingEngine();

        decimal quantityCollected = 0.0M;

        foreach (var order in dicOrderToMatch.Values)
        {
            if (command.Order.LeavesQuantity > (quantityCollected + order.LeavesQuantity))
            {
                quantityCollected += order.LeavesQuantity;

                if (command.Order.LeavesQuantity == quantityCollected)
                    command.Order.OrderStatus = OrderStatus.Filled;

                orderToExecute.PrincipalOrder = command.Order;
                orderToExecute.PartOrders.Add(order.OrderID, order);
            }
        }

        if (command.Order.OrderStatus != OrderStatus.Filled) //se não deu filled então cancela a operação
        {
            orderToExecute.PrincipalOrder.OrderStatus = OrderStatus.Cancelled;
            orderToExecute.PartOrders.Clear();
        }
        else if (command.Order.OrderStatus == OrderStatus.Filled)
        {
            foreach (var order in orderToExecute.PartOrders.Values)
            {
                //command.Order.LeavesQuantity -= order.LeavesQuantity;
                order.LeavesQuantity = 0;
                order.OrderStatus = OrderStatus.Filled;
                order.TransactTime = DateTime.Now;
                orderToExecute.PrincipalOrder.LastPrice = order.Price;
                order.LastPrice = order.Price;
            }
        }

        await _matchingCache.UpsertOrderMatchingAsync(orderToExecute, cancellation);

        return orderToExecute;
    }
}
