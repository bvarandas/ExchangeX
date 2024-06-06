using SharedX.Core.Entities;
using SharedX.Core.Events;
namespace Security.Application.Events;
public class SecurityChangedEvent : Event
{
    public SecurityEngine Security = null!;
    public SecurityChangedEvent(SecurityEngine securityEngine)
    {
        Security = securityEngine;
    }
}