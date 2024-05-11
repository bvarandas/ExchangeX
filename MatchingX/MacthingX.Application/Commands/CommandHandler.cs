using FluentValidation.Results;
using MatchingX.Core.Notifications;
using MediatR;
using SharedX.Core.Bus;
using SharedX.Core.Commands;
using SharedX.Core.Entities;

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
        ReportFix report = null!;
        
        //switch(message)
        //{
        //    case OrderTradeCancelCommand:
        //        report = new OrderCancelRejectFix();
        //        break;
        //    case OrderTradeModifyCommand:
        //        report = new BusinessMessageRejectFix();
        //        break;
        //    case OrderTradeNewCommand:
        //        report = new BusinessMessageRejectFix();
        //        break;
        //}

        foreach (var error in message.ValidationResult.Errors)
        {
            _bus.RaiseEvent(new DomainNotification(message.MessageType, error.ErrorMessage));
        }

        MakeReportWithErrors(ref report, message.ValidationResult.Errors);

        //_bus.RaiseEvent(new OrderTradeRejectEvent(report));
    }

    private void MakeReportWithErrors(ref ReportFix report, List<ValidationFailure> failures)
    {
        foreach (var failure in failures)
        {

        }
    }
}