using MediatR;
using SecurityX.Core.Notifications;
using SharedX.Core.Bus;

namespace Security.Application.Commands;
public class SecurityEngineCommandHandler : 
    CommandHandler,
    IRequestHandler<SecurityNewCommand, bool>,
    IRequestHandler<SecurityRemoveCommand, bool>,
    IRequestHandler<SecurityUpdateCommand, bool>
{
    public SecurityEngineCommandHandler(IMediatorHandler bus, INotificationHandler<DomainNotification> notifications) : base(bus, notifications)
    {
    }
}
