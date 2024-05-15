
using FluentValidation.Results;
using MediatR;
using SharedX.Core.Enums;

namespace SharedX.Core.Commands;

public abstract class MatchCommand : IRequest<OrderStatus>, INotification
{
    public DateTime Timestamp { get; private set; }
    public ValidationResult ValidationResult { get; set; }
    public string MessageType { get; protected set; }
    protected MatchCommand()
    {
        MessageType = GetType().Name;
        Timestamp = DateTime.Now;
    }
    public abstract bool IsValid();
}
