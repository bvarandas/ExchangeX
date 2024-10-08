﻿using Amazon.Runtime.Internal.Util;
using Security.Application.Commands;
using Security.Application.Validations;
using SecurityX.Core.Interfaces;
using SharedX.Core.Entities;
namespace SecurityX.Core.Notifications;
public class SecurityNewCommand : SecurityEngineCommand
{
    private readonly ISecurityCache _securityCache = null!;
    public SecurityNewCommand(SecurityEngine securityEngine, ISecurityCache securityCache,CancellationToken cancellationToken)
    {
        SecurityEngine = securityEngine;
        CancellationToken = cancellationToken;
        _securityCache = securityCache;
    }

    public override bool IsValid()
    {
        ValidationResult = new SecurityNewValidation(_securityCache).Validate(this);
        return ValidationResult.IsValid;
    }
}
