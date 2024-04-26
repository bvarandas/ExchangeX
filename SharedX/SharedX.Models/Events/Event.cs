using MediatR;
namespace SharedX.Core.Events;
public abstract class Event : IRequest<bool>, INotification
{
    public DateTime Timestamp { get; private set; }
    protected Event()
    {
        Timestamp = DateTime.Now;
    }
}