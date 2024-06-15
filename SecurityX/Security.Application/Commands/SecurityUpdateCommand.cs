using Security.Application.Validations;
using SecurityX.Core.Interfaces;
using SharedX.Core.Entities;
namespace Security.Application.Commands;
public class SecurityUpdateCommand : SecurityEngineCommand
{
    private readonly ISecurityCache _securityCache = null!;
    public SecurityUpdateCommand(SecurityEngine securityEngine, ISecurityCache securityCache, CancellationToken cancellationToken)
    {
        SecurityEngine = securityEngine;
        CancellationToken = cancellationToken;
        _securityCache = securityCache;
    }

    public override bool IsValid()
    {
        ValidationResult = new SecurityUpdateValidation(_securityCache).Validate(this);
        return ValidationResult.IsValid;
    }
}
