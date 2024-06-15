using SecurityX.Core.Interfaces;
using SecurityX.Core.Notifications;
namespace Security.Application.Validations;
public class SecurityNewValidation : SecurityValidation<SecurityNewCommand>
{
    public SecurityNewValidation(ISecurityCache securityCache ) : base( securityCache )
    {
        ValidateNew();
    }
}
