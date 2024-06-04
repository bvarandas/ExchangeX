using Security.Application.Validations;
using SharedX.Core.Entities;
namespace Security.Application.Commands;
public class SecurityUpdateCommand : SecurityEngineCommand
{
    public SecurityUpdateCommand(SecurityEngine securityEngine)
    {
        SecurityEngine = securityEngine;
    }

    public override bool IsValid()
    {
        ValidationResult = new SecurityUpdateValidation().Validate(this);
        return ValidationResult.IsValid;
    }
}
