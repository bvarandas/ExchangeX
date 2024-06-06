using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityX.Core.Notifications;
namespace Security.API.Controllers;
public class BaseController : Controller
{
    private readonly DomainNotificationHandler _domainNotificationHandler =null!;
    public BaseController(INotificationHandler<DomainNotification> notifications )
    {
        _domainNotificationHandler = (DomainNotificationHandler)notifications;
    }
    public bool IsValidOperation()
    {
        return (!_domainNotificationHandler.HasNotifications());
    }
}