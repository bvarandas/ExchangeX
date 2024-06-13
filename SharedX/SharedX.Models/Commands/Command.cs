using MediatR;
using FluentValidation.Results;
using FluentResults;

namespace SharedX.Core.Commands;
public abstract class Command : IRequest<Result>, INotification
{
    public DateTime Timestamp { get; private set; }
    public ValidationResult ValidationResult { get; set; }
    public string MessageType { get; protected set; }
    protected Command()
    {
        MessageType = GetType().Name;
        Timestamp = DateTime.Now;
    }
    public abstract bool IsValid();
}
