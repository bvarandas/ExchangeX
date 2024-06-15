using FluentResults;
using MediatR;
using MongoDB.Bson;
using Security.Application.Events;
using SecurityX.Core.Interfaces;
using SecurityX.Core.Notifications;
using SharedX.Core.Bus;
namespace Security.Application.Commands;
public class SecurityEngineCommandHandler : 
    CommandHandler,
    IRequestHandler<SecurityNewCommand, Result>,
    IRequestHandler<SecurityRemoveCommand, Result>,
    IRequestHandler<SecurityUpdateCommand, Result>
{
    private readonly ISecurityEngineRepository _securityRepository;
    private readonly IMediatorHandler _bus = null!;
    public SecurityEngineCommandHandler(IMediatorHandler bus,
        ISecurityEngineRepository securityRepository,
        INotificationHandler<DomainNotification> notifications) 
        : base(bus, notifications)
    {
        _securityRepository = securityRepository;
        _bus = bus;
    }
    public async Task<Result> Handle(SecurityNewCommand command, CancellationToken cancellationToken)
    {
        if (!command.IsValid())
        {
            NotifyValidationErrors(command);
            return Result.Fail(new Error(GetNotifyValidationErrors(command)));
        }
        command.SecurityEngine.Id = ObjectId.GenerateNewId().ToString();
        var result = await _securityRepository.UpsertSecurityAsync(command.SecurityEngine, cancellationToken);

        if (result.IsSuccess)
        {
            await _bus.Publish(new SecurityChangedEvent(command.SecurityEngine));
            return Result.Ok();
        }
        return result;
    }
    public async Task<Result> Handle(SecurityRemoveCommand command, CancellationToken cancellationToken)
    {
        if (!command.IsValid())
        {
            NotifyValidationErrors(command);
            return Result.Fail(new Error(GetNotifyValidationErrors(command)));
        }
        var result = await _securityRepository.RemoveSecurityAsync(command.SecurityEngine, cancellationToken);

        if (result.IsSuccess)
        {
            await _bus.Publish(new SecurityChangedEvent(command.SecurityEngine));
            return Result.Ok();
        }
        return result;
    }
    public async Task<Result> Handle(SecurityUpdateCommand command, CancellationToken cancellationToken)
    {
        if (!command.IsValid())
        {
            NotifyValidationErrors(command);
            return Result.Fail(new Error(GetNotifyValidationErrors(command)));
        }
        var result = await _securityRepository.UpsertSecurityAsync(command.SecurityEngine, cancellationToken);

        if (result.IsSuccess)
        {
            await _bus.Publish(new SecurityChangedEvent(command.SecurityEngine));
            return Result.Ok();
        }
        return result;
    }
}