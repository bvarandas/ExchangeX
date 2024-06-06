using MediatR;
using Microsoft.Extensions.Logging;
using SecurityX.Core.Interfaces;
namespace Security.Application.Events;
public class SecurityEngineEventHandler :
    INotificationHandler<SecurityChangedEvent>
{
    private readonly ISecurityCache _securityCache = null!;
    private readonly ILogger<SecurityEngineEventHandler> _logger;
    public SecurityEngineEventHandler(ILogger<SecurityEngineEventHandler> logger, ISecurityCache securityCache)
    {
        _securityCache = securityCache;
        _logger = logger;
    }
    public async Task Handle(SecurityChangedEvent notification, CancellationToken cancellationToken)
    {
        await _securityCache.UpsertSecurityAsync(notification.Security);
    }
}