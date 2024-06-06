using SharedX.Core.Commands;
using SharedX.Core.Entities;
namespace Security.Application.Commands;
public abstract  class SecurityEngineCommand : Command
{
    public CancellationToken CancellationToken { get; set; }
    public SecurityEngine SecurityEngine { get; protected set; } = new SecurityEngine();
    public DateTime Timestamp { get; protected set; } = DateTime.Now;
}