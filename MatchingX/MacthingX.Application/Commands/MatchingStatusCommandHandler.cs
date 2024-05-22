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
    IRequestHandler<MatchingOpenedCommand, bool>,
    IRequestHandler<MatchingFilledCommand, bool>,
    IRequestHandler<MatchingPartiallyFilledCommand, bool>,
    IRequestHandler<MatchingCancelCommand, bool>
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
    public Task<bool> Handle(MatchingOpenedCommand request, CancellationToken cancellationToken)
    {
        var matchResult = _matchRepository.UpsertOrderMatchingAsync(request.Order, cancellationToken);
        return matchResult;
    }
    public Task<bool> Handle(MatchingFilledCommand request, CancellationToken cancellationToken)
    {
        var listOrderId = new List<long>() { request.Order.OrderID };
        var matchResult = _matchRepository.RemoveOrdersMatchingAsync(listOrderId, cancellationToken);
        return matchResult;
    }
    public Task<bool> Handle(MatchingPartiallyFilledCommand request, CancellationToken cancellationToken)
    {
        var matchResult = _matchRepository.UpsertOrderMatchingAsync(request.Order, cancellationToken);
        return matchResult;
    }
    public Task<bool> Handle(MatchingCancelCommand request, CancellationToken cancellationToken)
    {
        var listOrderId = new List<long>() { request.Order.OrderID };
        var matchResult = _matchRepository.RemoveOrdersMatchingAsync(listOrderId, cancellationToken);
        return matchResult;
    }
}