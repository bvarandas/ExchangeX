using MacthingX.Application.Commands.Match;
using MatchingX.Application.Commands;
using MatchingX.Core.Interfaces;
using MatchingX.Core.Notifications;
using MatchingX.Core.Repositories;
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

    public MatchingCommandHandler(IMatchingRepository repository,
        IExecutedTradeRepository tradeRepository,
        IMediatorHandler bus,
        INotificationHandler<DomainNotification> notifications)
        : base(bus, notifications)
    {
        _tradeRepository = tradeRepository; 
        _matchRepository = repository;
        _bus = bus;
    }

    public async Task<(OrderStatus, Dictionary<long, OrderEngine>)> Handle(MatchingLimitCommand command, CancellationToken cancellationToken)
    {
        var resultMatch = await _matchRepository.MatchingLimitAsync(command.Order, cancellationToken);

        return resultMatch;
    }

    public async Task<(OrderStatus, Dictionary<long, OrderEngine>)> Handle(MatchingMarketCommand command, CancellationToken cancellationToken)
    {
        var resultMatch = await _matchRepository.MatchingLimitAsync(command.Order, cancellationToken);

        return resultMatch;
    }

    public async Task<(OrderStatus, Dictionary<long, OrderEngine>)> Handle(MatchingStopLimitCommand command, CancellationToken cancellationToken)
    {
        var resultMatch = await _matchRepository.MatchingLimitAsync(command.Order, cancellationToken);

        return resultMatch;
    }

    public async Task<(OrderStatus, Dictionary<long, OrderEngine>)> Handle(MatchingStopCommand command, CancellationToken cancellationToken)
    {
        var resultMatch = await _matchRepository.MatchingLimitAsync(command.Order, cancellationToken);

        return resultMatch;
    }
}
