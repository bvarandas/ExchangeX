
using FluentValidation.Results;
using MatchingX.Core.Entities;
using MediatR;

namespace MacthingX.Application.Commands.Match;

public abstract class MatchCommand : IRequest<MatchingEngine>, INotification
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
