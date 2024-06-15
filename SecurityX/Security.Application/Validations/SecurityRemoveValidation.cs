using Security.Application.Commands;
using SecurityX.Core.Interfaces;
namespace Security.Application.Validations;
public class SecurityRemoveValidation : SecurityValidation<SecurityRemoveCommand>
{
    public SecurityRemoveValidation(ISecurityCache securityCache) : base(securityCache)
    {
        ValidateRemove();
    }
}