using FluentValidation.Results;
using MacthingX.Application.Commands;
using MacthingX.Application.Events;
using MatchingX.Core.Notifications;
using MediatR;
using SharedX.Core.Bus;
using SharedX.Core.Commands;
using SharedX.Core.Entities;
using SharedX.Core.Extensions;

namespace MatchingX.Application.Commands;
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
            _bus.RaiseEvent(new DomainNotification(message.MessageType, error.ErrorMessage));
        }

        MakeReportWithErrors(message,message.ValidationResult.Errors);

        //_bus.RaiseEvent(new OrderTradeRejectEvent(report));
    }
    
    private void MakeReportWithErrors(Command message, List<ValidationFailure> failures)
    {
        var report = new ReportFix();
        switch (message)
        {
            case OrderCancelCommand:
                {
                    foreach (var failure in failures)
                    {
                        int CxlRejReason = int.Parse(failure.ErrorCode);
                        report = ((OrderCancelCommand)message)
                            .Order.ReportOrderCancelRejectFix(CxlRejReason, '1', failure.ErrorMessage);
                        _bus.RaiseEvent(new OrderRejectEvent(report));
                    }

                }
                break;
            case OrderCancelReplaceCommand:
                {
                    foreach (var failure in failures)
                    {
                        if (failure.PropertyName != "replace")
                        {
                            int CxlRejReason = int.Parse(failure.ErrorCode);
                            report = ((OrderCancelCommand)message)
                                .Order.ReportOrderCancelRejectFix(CxlRejReason, '1', failure.ErrorMessage);
                            _bus.RaiseEvent(new OrderRejectEvent(report));
                        }
                        else
                        {
                            int BusinessRejectReason = int.Parse(failure.ErrorCode);
                            report = ((OrderCancelCommand)message)
                                .Order.ReportBusinessMessageRejectFix(BusinessRejectReason, "x", failure.ErrorMessage);
                            _bus.RaiseEvent(new OrderRejectEvent(report));
                        }
                    }
                    
                }
                break;
            case OrderOpenedCommand:
                {
                    foreach (var failure in failures)
                    {
                        int BusinessRejectReason = int.Parse(failure.ErrorCode);
                        report = ((OrderOpenedCommand)message)
                            .Order.ReportBusinessMessageRejectFix(BusinessRejectReason, "1", failure.ErrorMessage);
                        _bus.RaiseEvent(new OrderRejectEvent(report));
                    }
                    
                }
                break;
        }
    }

    
}