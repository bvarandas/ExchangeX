using SharedX.Core.Commands;
using SharedX.Core.Events;
using SharedX.Core.Querys;

namespace SharedX.Core.Bus;
public interface IMediatorHandler
{
    Task SendCommand<T>(T command) where T : Command;
    Task RaiseEvent<T>(T @event) where T : Event;
    Task<R> QueryReply<Q,R>(Q query) where Q: Query where R: class;
}