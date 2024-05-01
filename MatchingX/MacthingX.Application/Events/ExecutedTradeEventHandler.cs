using MatchingX.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
namespace MacthingX.Application.Events;
public class ExecutedTradeEventHandler : 
    INotificationHandler<ExecutedTradeEvent>
{
    private readonly IDropCopyCache _cache;
    private readonly ILogger<ExecutedTradeEventHandler> _logger;
    public ExecutedTradeEventHandler(ILogger<ExecutedTradeEventHandler> logger, IDropCopyCache cache)
    {
        _logger = logger;
        _cache = cache;
    }

    public async Task Handle(ExecutedTradeEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Enviando executed Trade para o Cache Drop Copy");
        _cache.AddTradeCaptureReport(notification.ExecutedTrade.Item1);
        _cache.AddTradeCaptureReport(notification.ExecutedTrade.Item2);

    }
}