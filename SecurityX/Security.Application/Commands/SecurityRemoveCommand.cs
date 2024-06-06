using Security.Application.Validations;
using SharedX.Core.Entities;
namespace Security.Application.Commands;
public class SecurityRemoveCommand : SecurityEngineCommand
{
    public SecurityRemoveCommand(SecurityEngine securityEngine, CancellationToken cancellationToken)
    {
        SecurityEngine = securityEngine;
        CancellationToken = cancellationToken;
    }
    public override bool IsValid()
    {
        ValidationResult = new SecurityRemoveValidation().Validate(this);
        return ValidationResult.IsValid;
    }
}
