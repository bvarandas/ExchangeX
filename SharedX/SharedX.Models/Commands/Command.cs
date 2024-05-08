using MediatR;
using FluentValidation.Results;
namespace SharedX.Core.Commands;
public abstract class Command : IRequest<bool>, INotification
{
    public DateTime Timestamp { get; private set; }
    public ValidationResult ValidationResult { get; set; }
    protected Command()
    {
        Timestamp = DateTime.Now;
    }
    public abstract bool IsValid();
}
