using Amazon.Runtime.Internal.Util;
using Security.Application.Commands;
using Security.Application.Validations;
using SharedX.Core.Entities;
namespace SecurityX.Core.Notifications;
public class SecurityNewCommand : SecurityEngineCommand
{

    public SecurityNewCommand(SecurityEngine securityEngine)
    {
        SecurityEngine = securityEngine;
    }

    public override bool IsValid()
    {
        ValidationResult = new SecurityNewValidation().Validate(this);
        return ValidationResult.IsValid;
    }
}
