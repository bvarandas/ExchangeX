using Security.Application.Validations;
using SecurityX.Core.Interfaces;
using SharedX.Core.Entities;
namespace Security.Application.Commands;
public class SecurityRemoveCommand : SecurityEngineCommand
{
    private readonly ISecurityCache _securityCache = null!;
    public SecurityRemoveCommand(SecurityEngine securityEngine, ISecurityCache securityCache,CancellationToken cancellationToken)
    {
        SecurityEngine = securityEngine;
        CancellationToken = cancellationToken;
        _securityCache = securityCache;
    }
    public override bool IsValid()
    {
        ValidationResult = new SecurityRemoveValidation(_securityCache).Validate(this);
        return ValidationResult.IsValid;
    }
}
