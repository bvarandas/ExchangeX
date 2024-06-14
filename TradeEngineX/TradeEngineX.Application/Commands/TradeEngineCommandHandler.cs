using FluentResults;
using MediatR;
using SharedX.Core.Bus;
using TradeEngineX.Application.Events;
using TradeEngineX.Core.Interfaces;
using TradeEngineX.Core.Notifications;
namespace TradeEngineX.Application.Commands;
public class TradeEngineCommandHandler :
    CommandHandler,
    IRequestHandler<TradeEngineNewCommand, Result>,
    IRequestHandler<TradeEngineRemoveCommand, Result>,
    IRequestHandler<TradeEngineUpdateCommand, Result>
{
    private readonly ITradeEngineRepository _tradeRepository;
    private readonly IMediatorHandler _bus = null!;

    public TradeEngineCommandHandler(IMediatorHandler bus,
        ITradeEngineRepository tradeRepository,
        INotificationHandler<DomainNotification> notifications)
        : base(bus, notifications)
    {
        _tradeRepository = tradeRepository;
        _bus = bus;
    }

    public async Task<Result> Handle(TradeEngineNewCommand command, CancellationToken cancellationToken)
    {
        var result = await _tradeRepository.UpsertTradeAsync(command.TradeReport, cancellationToken);

        if (result.IsSuccess)
            await _bus.Publish(new TradeEngineCreatedEvent(command.TradeReport));

        return Result.Ok();
    }

    public async Task<Result> Handle(TradeEngineRemoveCommand command, CancellationToken cancellationToken)
    {
        var result = await _tradeRepository.UpsertTradeAsync(command.TradeReport, cancellationToken);

        if (result.IsSuccess)
            await _bus.Publish(new TradeEngineRemovedEvent(command.TradeReport));

        return Result.Ok();
    }

    public async Task<Result> Handle(TradeEngineUpdateCommand command, CancellationToken cancellationToken)
    {
        var result = await _tradeRepository.UpsertTradeAsync(command.TradeReport, cancellationToken);

        if (result.IsSuccess)
            await _bus.Publish(new TradeEngineUpdatedEvent(command.TradeReport));

        return Result.Ok();
    }
}