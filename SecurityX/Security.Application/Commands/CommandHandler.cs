using MediatR;
using SecurityX.Core.Notifications;
using SharedX.Core.Bus;

namespace Security.Application.Commands;
public class CommandHandler
{
    private readonly IMediatorHandler _bus;
    private readonly DomainNotificationHandler _notifications;
    public CommandHandler(IMediatorHandler bus, INotificationHandler<DomainNotification> notifications)
    {
        _bus = bus;

        _notifications = (DomainNotificationHandler)notifications;
    }



}
