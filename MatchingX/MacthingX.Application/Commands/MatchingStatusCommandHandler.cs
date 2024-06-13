using FluentResults;
using MacthingX.Application.Commands.Match.OrderStatus;
using MatchingX.Application.Commands;
using MatchingX.Core.Interfaces;
using MatchingX.Core.Notifications;
using MatchingX.Core.Repositories;
using MediatR;
using SharedX.Core.Bus;
namespace MacthingX.Application.Commands;
public class MatchingStatusCommandHandler : 
    CommandHandler,
    IRequestHandler<MatchingOpenedCommand, Result>,
    IRequestHandler<MatchingFilledCommand, Result>,
    IRequestHandler<MatchingPartiallyFilledCommand, Result>,
    IRequestHandler<MatchingCancelCommand, Result>
{
    private readonly IMediatorHandler _bus;
    private readonly IMatchingRepository _matchRepository;
    private readonly IExecutedTradeRepository _tradeRepository;
    public MatchingStatusCommandHandler(
        IMatchingRepository matchRepository,
        IExecutedTradeRepository tradeRepository,
        IMediatorHandler bus,
        INotificationHandler<DomainNotification> notifications) :base(bus, notifications)
    {
        _bus = bus;
        _tradeRepository = tradeRepository;
        _matchRepository = matchRepository;
    }
    public async Task<Result> Handle(MatchingOpenedCommand request, CancellationToken cancellationToken)
    {
        var matchResult = await _matchRepository.UpsertOrderMatchingAsync(request.Order, cancellationToken);
        return matchResult;
    }
    public async Task<Result> Handle(MatchingFilledCommand request, CancellationToken cancellationToken)
    {
        var listOrderId = new List<long>() { request.Order.OrderID };
        var matchResult = await _matchRepository.RemoveOrdersMatchingAsync(listOrderId, cancellationToken);
        return matchResult;
    }
    public async Task<Result> Handle(MatchingPartiallyFilledCommand request, CancellationToken cancellationToken)
    {
        var matchResult = await _matchRepository.UpsertOrderMatchingAsync(request.Order, cancellationToken);
        return matchResult;
    }
    public async Task<Result> Handle(MatchingCancelCommand request, CancellationToken cancellationToken)
    {
        var listOrderId = new List<long>() { request.Order.OrderID };
        var matchResult = await _matchRepository.RemoveOrdersMatchingAsync(listOrderId, cancellationToken);
        return matchResult;
    }
}