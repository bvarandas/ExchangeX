﻿using MediatR;
namespace TradeEngineX.Core.Notifications;
public class DomainNotificationHandler : INotificationHandler<DomainNotification>
{
    private List<DomainNotification> _notifications;

    public DomainNotificationHandler()
    {
        _notifications = new List<DomainNotification>();
    }

    public Task Handle(DomainNotification notification, CancellationToken cancellationToken)
    {
        _notifications.Add(notification);

        return Task.CompletedTask;
    }

    public virtual List<DomainNotification> GetNotifications()
    {
        return _notifications;
    }

    public virtual bool HasNotifications()
    {
        return GetNotifications().Any();
    }

    public void Dispose()
    {
        _notifications.Clear();
    }
}