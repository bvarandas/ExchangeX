using MacthingX.Application.Commands.Match;
using MatchingX.Application.Commands;
using MatchingX.Core.Interfaces;
using MatchingX.Core.Notifications;
using MediatR;
using SharedX.Core.Bus;
using SharedX.Core.Enums;
namespace MacthingX.Application.Commands;
public class MatchingCommandHandler :
    CommandHandler,
    IRequestHandler<MatchingLimitCommand, OrderStatus>,
    IRequestHandler<MatchingMarketCommand, OrderStatus>,
    IRequestHandler<MatchingStopLimitCommand, OrderStatus>,
    IRequestHandler<MatchingStopCommand, OrderStatus>
{
    private readonly IMatchingRepository _repository;
    private readonly IMediatorHandler _bus;

    public MatchingCommandHandler(IMatchingRepository repository,
        IMediatorHandler bus,
        INotificationHandler<DomainNotification> notifications)
        : base(bus, notifications)
    {
        _repository = repository;
        _bus = bus;
    }

    public async Task<OrderStatus> Handle(MatchingLimitCommand command, CancellationToken cancellationToken)
    {
        return OrderStatus.Filled;
    }

    public async Task<OrderStatus> Handle(MatchingMarketCommand request, CancellationToken cancellationToken)
    {
        return OrderStatus.Filled;
    }

    public async Task<OrderStatus> Handle(MatchingStopLimitCommand request, CancellationToken cancellationToken)
    {
        return OrderStatus.Filled;
    }

    public async Task<OrderStatus> Handle(MatchingStopCommand request, CancellationToken cancellationToken)
    {
        return OrderStatus.Filled;
    }
}
