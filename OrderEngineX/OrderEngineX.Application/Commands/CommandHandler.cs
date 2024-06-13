using FluentValidation.Results;
using MediatR;
using OrderEngineX.Application.Commands.Order;
using OrderEngineX.Application.Events;
using OrderEngineX.Core.Notifications;
using SharedX.Core.Bus;
using SharedX.Core.Commands;
using SharedX.Core.Entities;
using SharedX.Core.Extensions;
namespace OrderEngineX.Application.Commands;
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
        ReportFix report = null!;
        
        switch(message)
        {
            case OrderCancelCommand:
                report = new OrderCancelRejectFix();
                break;
            case OrderCancelReplaceCommand:
                report = new BusinessMessageRejectFix();
                break;
            case OrderOpenedCommand:
                report = new BusinessMessageRejectFix();
                break;
        }

        foreach (var error in message.ValidationResult.Errors)
        {
            _bus.Publish(new DomainNotification(message.MessageType, error.ErrorMessage));
        }

        MakeReportWithErrors(ref message, message.ValidationResult.Errors);

        _bus.Publish(new OrderTradeRejectedEvent(report));
    }

    private void MakeReportWithErrors(ref Command message, List<ValidationFailure> failures)
    {
        var report = new ReportFix();
        switch (message)
        {
            case OrderCancelCommand:
                {
                    foreach (var failure in failures)
                    {
                        int CxlRejReason = int.Parse(failure.ErrorCode);
                        report = ((OrderCancelCommand)message).Order.ReportOrderCancelRejectFix(CxlRejReason, '1', failure.ErrorMessage);
                        _bus.Publish(new OrderTradeRejectedEvent(report));
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
                            _bus.Publish(new OrderTradeRejectedEvent(report));
                        }
                        else
                        {
                            int BusinessRejectReason = int.Parse(failure.ErrorCode);
                            report = ((OrderCancelCommand)message)
                                .Order.ReportBusinessMessageRejectFix(BusinessRejectReason, "x", failure.ErrorMessage);
                            _bus.Publish(new OrderTradeRejectedEvent(report));
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
                        _bus.Publish(new OrderTradeRejectedEvent(report));
                    }

                }
                break;
        }
    }
}