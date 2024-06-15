using Security.Application.Commands;
using SecurityX.Core.Interfaces;
namespace Security.Application.Validations;
public class SecurityUpdateValidation : SecurityValidation<SecurityUpdateCommand>
{
    public SecurityUpdateValidation(ISecurityCache securityCache) : base(securityCache)
    {
        ValidateUpdate();
    }
}