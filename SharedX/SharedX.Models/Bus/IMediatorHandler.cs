using SharedX.Core.Commands;
using SharedX.Core.Events;
namespace SharedX.Core.Bus;
public interface IMediatorHandler
{
    Task SendCommand<T>(T command) where T : Command;
    Task RaiseEvent<T>(T @event) where T : Event;
}