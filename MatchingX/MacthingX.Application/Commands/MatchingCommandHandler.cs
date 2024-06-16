using MacthingX.Application.Commands.Match.OrderType;
using MatchingX.Application.Commands;
using MatchingX.Core.Interfaces;
using MatchingX.Core.Notifications;
using MatchingX.Core.Repositories;
using Medallion.Threading;
using MediatR;
using SharedX.Core.Bus;
using SharedX.Core.Enums;
using SharedX.Core.Matching.OrderEngine;

namespace MacthingX.Application.Commands;
public class MatchingCommandHandler :
    CommandHandler,
    IRequestHandler<MatchingLimitCommand, (OrderStatus, Dictionary<long, OrderEngine>)>,
    IRequestHandler<MatchingMarketCommand, (OrderStatus, Dictionary<long, OrderEngine>)>,
    IRequestHandler<MatchingStopLimitCommand, (OrderStatus, Dictionary<long, OrderEngine>)>,
    IRequestHandler<MatchingStopCommand, (OrderStatus, Dictionary<long, OrderEngine>)>
{
    private readonly IMatchingRepository _matchRepository;
    private readonly IExecutedTradeRepository _tradeRepository;
    private readonly IMediatorHandler _bus;
    private readonly IDistributedLockProvider _distributedLockProvider;

    public MatchingCommandHandler(IMatchingRepository repository,
        IExecutedTradeRepository tradeRepository,
        IMediatorHandler bus,
        INotificationHandler<DomainNotification> notifications,
        IDistributedLockProvider distributedLockProvider)
        : base(bus, notifications)
    {
        _tradeRepository = tradeRepository; 
        _matchRepository = repository;
        _bus = bus;
        _distributedLockProvider = distributedLockProvider;
    }

    public async Task<(OrderStatus, Dictionary<long, OrderEngine>)> Handle(MatchingLimitCommand command, CancellationToken cancellationToken)
    {
        string nameLock = string.Concat(command.Order.Symbol, "_", command.Order.Side.ToString());
        await using (await _distributedLockProvider.TryAcquireLockAsync(nameLock,TimeSpan.FromSeconds(10) ,cancellationToken))
        {
            var resultMatch = await _matchRepository.MatchingLimitAsync(command.Order, cancellationToken);

            return resultMatch;
        }
    }

    public async Task<(OrderStatus, Dictionary<long, OrderEngine>)> Handle(MatchingMarketCommand command, CancellationToken cancellationToken)
    {
        string nameLock = string.Concat(command.Order.Symbol, "_", command.Order.Side.ToString());
        await using (await _distributedLockProvider.TryAcquireLockAsync(nameLock, TimeSpan.FromSeconds(10), cancellationToken))
        {
            var resultMatch = await _matchRepository.MatchingLimitAsync(command.Order, cancellationToken);

            return resultMatch;
        }
    }

    public async Task<(OrderStatus, Dictionary<long, OrderEngine>)> Handle(MatchingStopLimitCommand command, CancellationToken cancellationToken)
    {
        string nameLock = string.Concat(command.Order.Symbol, "_", command.Order.Side.ToString());
        await using (await _distributedLockProvider.TryAcquireLockAsync(nameLock, TimeSpan.FromSeconds(10), cancellationToken))
        {
            var resultMatch = await _matchRepository.MatchingLimitAsync(command.Order, cancellationToken);

            return resultMatch;
        }
    }

    public async Task<(OrderStatus, Dictionary<long, OrderEngine>)> Handle(MatchingStopCommand command, CancellationToken cancellationToken)
    {
        string nameLock = string.Concat(command.Order.Symbol, "_", command.Order.Side.ToString());
        await using (await _distributedLockProvider.TryAcquireLockAsync(nameLock, TimeSpan.FromSeconds(10), cancellationToken))
        {
            var resultMatch = await _matchRepository.MatchingLimitAsync(command.Order, cancellationToken);

            return resultMatch;
        }
    }
}
