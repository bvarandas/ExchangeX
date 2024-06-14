﻿using FluentResults;
using Microsoft.Extensions.Logging;
using Security.Application.Commands;
using SecurityX.Core.Interfaces;
using SecurityX.Core.Notifications;
using SharedX.Core.Bus;
using SharedX.Core.Entities;
namespace Security.Application.Services;
public class SecurityService : ISecurityService
{
    private readonly ISecurityEngineRepository _securityRepository;
    private readonly ILogger<SecurityService> _logger;
    private readonly IMediatorHandler _bus = null!;
    public SecurityService(IMediatorHandler bus,
        ISecurityEngineRepository securityRepository, 
        ILogger<SecurityService> logger)
    {
        _bus = bus;
        _logger = logger;   
        _securityRepository = securityRepository;
    }
    public async Task<Result<bool>> Add(SecurityEngine security, CancellationToken cancellationToken)
    {
        var result = await _bus.Send(new SecurityNewCommand(security, cancellationToken));

        return result;
    }
    public async Task<Result<bool>> Delete(SecurityEngine security, CancellationToken cancellationToken)
    {
        var result = await _bus.Send(new SecurityRemoveCommand(security, cancellationToken));
        return result;
    }
    public async Task<Result<Dictionary<string, SecurityEngine>>> Get(string[] ids, CancellationToken cancellationToken)
    {
        var result = await _securityRepository.GetAllSecurityiesAsync(cancellationToken);
        var dicResult = result.Value;

        if (ids != null && ids.Any())
        {
            dicResult = dicResult.Where(x => ids.Contains(x.Key)).ToDictionary(i => i.Key, i => i.Value);
        }
        return result;
    }
    public async Task<Result<bool>> Update(SecurityEngine security, CancellationToken cancellationToken)
    {
        var result = await _bus.Send(new SecurityUpdateCommand(security, cancellationToken));
        return result;
    }
}
public interface ISecurityService
{
    Task<Result<Dictionary<string, SecurityEngine>>> Get(string[] ids, CancellationToken cancellationToken);

    Task<Result<bool>> Add(SecurityEngine security, CancellationToken cancellationToken);

    Task<Result<bool>> Update(SecurityEngine security, CancellationToken cancellationToken);

    Task<Result<bool>> Delete(SecurityEngine security, CancellationToken cancellationToken);
}