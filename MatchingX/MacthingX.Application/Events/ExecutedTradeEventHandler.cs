using MediatR;
namespace MacthingX.Application.Events;
public class ExecutedTradeEventHandler : 
    INotificationHandler<ExecutedTradeEvent>
{
    public ExecutedTradeEventHandler()
    {

    }

    public async Task Handle(ExecutedTradeEvent notification, CancellationToken cancellationToken)
    {
        
    }
}