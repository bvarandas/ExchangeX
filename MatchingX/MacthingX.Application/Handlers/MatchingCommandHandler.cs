using MacthingX.Application.Commands.Match;
using MacthingX.Application.Commands.Match.OrderType;
using MatchingX.Core.Entities;
using MatchingX.Core.Interfaces;
using MatchingX.Core.Notifications;
using MatchingX.Core.Repositories;
using Medallion.Threading;
using MediatR;
using SharedX.Core.Bus;
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
    private readonly IMatchingCache _matchingCache;

    public MatchingCommandHandler(IMatchingRepository repository,
        IExecutedTradeRepository tradeRepository,
        IMediatorHandler bus,
        INotificationHandler<DomainNotification> notifications,
        IDistributedLockProvider distributedLockProvider,
        IMatchingCache matchingCache)
        : base(bus, notifications, matchingCache)
    {
        _matchingCache = matchingCache;
        _tradeRepository = tradeRepository;
        _matchRepository = repository;
        _bus = bus;
        _distributedLockProvider = distributedLockProvider;
    }

    public async Task<MatchingEngine> Handle(MatchingLimitCommand command, CancellationToken cancellationToken)
    {
        string nameLock = $"{command.Order.Symbol}_{command.Order.Side.ToString().ToLower()}";
        await using (await _distributedLockProvider.TryAcquireLockAsync(nameLock, TimeSpan.FromSeconds(3), cancellationToken))
        {
            var orderToMatch = command.Order.Side == SideTrade.Buy ?
                await _matchingCache.GetSellOrderBySymbol(command.Order.Symbol) :
                await _matchingCache.GetBuyOrderBySymbol(command.Order.Symbol);

            var orderToExecute = new MatchingEngine();

            var dicOrderToMatch = orderToMatch.Value
                                        .Where(i =>
                                        i.Value.LastQuantity <= command.Order.LastQuantity &&
                                        i.Value.Price <= command.Order.Price)
                                        .OrderBy(i => i.Value.Price)
                                        .ToDictionary(i => i.Key, i => i.Value);

            if (command.Order.TimeInForce != TimeInForce.FOK)
            {
                orderToExecute = await Execute(command, dicOrderToMatch, cancellationToken);
            }
            else if (command.Order.TimeInForce == TimeInForce.FOK)
            {
                orderToExecute = await ExecuteFok(command, dicOrderToMatch, cancellationToken);
            }

            return orderToExecute;
        }

    }

    public async Task<MatchingEngine> Handle(MatchingMarketCommand command, CancellationToken cancellationToken)
    {
        string nameLock = $"{command.Order.Symbol}_{command.Order.Side.ToString().ToLower()}";

        await using (await _distributedLockProvider.TryAcquireLockAsync(nameLock, TimeSpan.FromSeconds(3), cancellationToken))
        {
            var orderToMatch = command.Order.Side == SideTrade.Buy ?
                await _matchingCache.GetSellOrderBySymbol(command.Order.Symbol) :
                await _matchingCache.GetBuyOrderBySymbol(command.Order.Symbol);

            var orderToExecute = new MatchingEngine();

            var dicOrderToMatch = orderToMatch.Value
                                        .Where(i =>
                                        i.Value.LastQuantity <= command.Order.LastQuantity &&
                                        i.Value.Price <= command.Order.Price)
                                        .OrderBy(i => i.Value.Price)
                                        .ToDictionary(i => i.Key, i => i.Value);

            if (command.Order.TimeInForce != TimeInForce.FOK)
            {
                orderToExecute = await Execute(command, dicOrderToMatch, cancellationToken);
            }
            else if (command.Order.TimeInForce == TimeInForce.FOK)
            {
                orderToExecute = await ExecuteFok(command, dicOrderToMatch, cancellationToken);
            }

            return orderToExecute;
        }
    }

    public async Task<MatchingEngine> Handle(MatchingStopLimitCommand command, CancellationToken cancellationToken)
    {
        string nameLock = string.Concat(command.Order.Symbol, "_", command.Order.Side.ToString().ToLower());
        await using (await _distributedLockProvider.TryAcquireLockAsync(nameLock, TimeSpan.FromSeconds(3), cancellationToken))
        {
            var orderToMatch = command.Order.Side == SideTrade.Buy ?
                await _matchingCache.GetSellOrderBySymbol(command.Order.Symbol) :
                await _matchingCache.GetBuyOrderBySymbol(command.Order.Symbol);

            var orderToExecute = new MatchingEngine();

            var dicOrderToMatch = orderToMatch.Value
                                        .Where(i =>
                                        i.Value.LastQuantity <= command.Order.LastQuantity &&
                                        i.Value.Price <= command.Order.Price)
                                        .OrderBy(i => i.Value.Price)
                                        .ToDictionary(i => i.Key, i => i.Value);

            if (command.Order.TimeInForce != TimeInForce.FOK)
            {
                orderToExecute = await Execute(command, dicOrderToMatch, cancellationToken);
            }
            else if (command.Order.TimeInForce == TimeInForce.FOK)
            {
                orderToExecute = await ExecuteFok(command, dicOrderToMatch, cancellationToken);
            }



            return orderToExecute;
        }
    }

    public async Task<MatchingEngine> Handle(MatchingStopCommand command, CancellationToken cancellationToken)
    {
        string nameLock = $"{command.Order.Symbol}_{command.Order.Side.ToString().ToLower()}";

        await using (await _distributedLockProvider.TryAcquireLockAsync(nameLock, TimeSpan.FromSeconds(3), cancellationToken))
        {
            var orderToMatch = command.Order.Side == SideTrade.Buy ?
                await _matchingCache.GetSellOrderBySymbol(command.Order.Symbol) :
                await _matchingCache.GetBuyOrderBySymbol(command.Order.Symbol);

            var orderToExecute = new MatchingEngine();

            var dicOrderToMatch = orderToMatch.Value
                                        .Where(i =>
                                        i.Value.LastQuantity <= command.Order.LastQuantity &&
                                        i.Value.Price <= command.Order.Price)
                                        .OrderBy(i => i.Value.Price)
                                        .ToDictionary(i => i.Key, i => i.Value);

            if (command.Order.TimeInForce != TimeInForce.FOK)
            {
                orderToExecute = await Execute(command, dicOrderToMatch, cancellationToken);
            }
            else if (command.Order.TimeInForce == TimeInForce.FOK)
            {
                orderToExecute = await ExecuteFok(command, dicOrderToMatch, cancellationToken);
            }

            return orderToExecute;
        }
    }

    private async Task<MatchingEngine> Execute(MatchingEngineCommand command,
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

        if (command.Order.Side == SideTrade.Sell)
            await _matchingCache.UpsertSellOrderMatchingAsync(orderToExecute, cancellation);
        else
            await _matchingCache.UpsertBuyOrderMatchingAsync(orderToExecute, cancellation);

        return orderToExecute;
    }

    private async Task<MatchingEngine> ExecuteFok(MatchingEngineCommand command,
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

        if (orderToExecute.PrincipalOrder.Side == SideTrade.Buy)
            await _matchingCache.UpsertBuyOrderMatchingAsync(orderToExecute, cancellation);
        else
            await _matchingCache.UpsertSellOrderMatchingAsync(orderToExecute, cancellation);

        return orderToExecute;
    }
}
