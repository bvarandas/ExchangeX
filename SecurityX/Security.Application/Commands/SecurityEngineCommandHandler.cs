using MediatR;
using Security.Application.Events;
using SecurityX.Core.Interfaces;
using SecurityX.Core.Notifications;
using SharedX.Core.Bus;
namespace Security.Application.Commands;
public class SecurityEngineCommandHandler : 
    CommandHandler,
    IRequestHandler<SecurityNewCommand, bool>,
    IRequestHandler<SecurityRemoveCommand, bool>,
    IRequestHandler<SecurityUpdateCommand, bool>
{
    private readonly ISecurityRepository _securityRepository;
    private readonly IMediatorHandler _bus = null!;
    public SecurityEngineCommandHandler(IMediatorHandler bus,
        ISecurityRepository securityRepository,
        INotificationHandler<DomainNotification> notifications) 
        : base(bus, notifications)
    {
        _securityRepository = securityRepository;
        _bus = bus;
    }
    public async Task<bool> Handle(SecurityNewCommand command, CancellationToken cancellationToken)
    {
        if (!command.IsValid())
        {
            NotifyValidationErrors(command);
            return false;
        }
        var result = await _securityRepository.UpsertSecurityAsync(command.SecurityEngine, cancellationToken);
        
        if (result.IsSuccess)
            await _bus.RaiseEvent(new SecurityChangedEvent(command.SecurityEngine));

        return result.IsSuccess;
    }
    public async Task<bool> Handle(SecurityRemoveCommand command, CancellationToken cancellationToken)
    {
        if (!command.IsValid())
        {
            NotifyValidationErrors(command);
            return false;
        }
        var result = await _securityRepository.UpsertSecurityAsync(command.SecurityEngine, cancellationToken);

        if (result.IsSuccess)
            await _bus.RaiseEvent(new SecurityChangedEvent(command.SecurityEngine));

        return result.IsSuccess;
    }
    public async Task<bool> Handle(SecurityUpdateCommand command, CancellationToken cancellationToken)
    {
        if (!command.IsValid())
        {
            NotifyValidationErrors(command);
            return false;
        }
        var result = await _securityRepository.UpsertSecurityAsync(command.SecurityEngine, cancellationToken);

        if (result.IsSuccess)
            await _bus.RaiseEvent(new SecurityChangedEvent(command.SecurityEngine));

        return result.IsSuccess;
    }
}