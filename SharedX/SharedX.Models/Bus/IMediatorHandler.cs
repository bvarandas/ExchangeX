using SharedX.Core.Commands;
using SharedX.Core.Enums;
using SharedX.Core.Events;
using SharedX.Core.Matching.OrderEngine;
using System.Collections;

namespace SharedX.Core.Bus;
public interface IMediatorHandler
{
    Task SendCommand<T>(T command) where T : Command;
    Task RaiseEvent<T>(T @event) where T : Event;
    Task<(OrderStatus, Dictionary<long, OrderEngine>)> SendMatchCommand<T>(T command) where T : MatchCommand;
                                              //where R : (OrderStatus, Dictionary<long, OrderEngine>);// Tuple<OrderStatus, Dictionary<long, OrderEngine>>; 
}