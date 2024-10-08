﻿using MediatR;
using SecurityX.Core.Notifications;
using SharedX.Core.Bus;
using SharedX.Core.Commands;
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
    protected void NotifyValidationErrors(Command message)
    {
        foreach (var error in message.ValidationResult.Errors)
        {
            _bus.Publish(new DomainNotification(message.MessageType, error.ErrorMessage));
        }
    }

    protected string GetNotifyValidationErrors(Command message)
    {
        string listError = string.Empty;
        
        foreach (var error in message.ValidationResult.Errors)
        {
            listError = string.Concat(message.MessageType, ":", error.ErrorMessage);
        }
        return listError;
    }
}