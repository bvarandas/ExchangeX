using Security.Application.Validations;
using SharedX.Core.Entities;
namespace Security.Application.Commands;
public class SecurityUpdateCommand : SecurityEngineCommand
{
    public SecurityUpdateCommand(SecurityEngine securityEngine,  CancellationToken cancellationToken)
    {
        SecurityEngine = securityEngine;
        CancellationToken = cancellationToken;
    }

    public override bool IsValid()
    {
        ValidationResult = new SecurityUpdateValidation().Validate(this);
        return ValidationResult.IsValid;
    }
}
