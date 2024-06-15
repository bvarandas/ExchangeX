using FluentResults;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Security.Infra.Data;
using SecurityX.Core.Interfaces;
using SharedX.Core.Entities;
namespace Security.Infra.Repositories;

public class SecurityEngineRepository : ISecurityEngineRepository
{
    private readonly ISecurityEngineContext _context;
    private readonly ILogger<SecurityEngineRepository> _logger;

    public SecurityEngineRepository(ISecurityEngineContext context, ILogger<SecurityEngineRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Dictionary<string, SecurityEngine>>> GetAllSecurityiesAsync(CancellationToken cancellation)
    {
        Dictionary<string, SecurityEngine> result = null!;
        try
        {
            var builder = Builders<SecurityEngine>.Filter;
            var filter = builder.Empty;
            var securities = _context.SecurityEngine.Find(filter).ToEnumerable();
            
            result = new Dictionary<string, SecurityEngine>();

            foreach (var security in securities)
                result.Add(security.SecurityID, security);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return Result.Fail(new Error(ex.Message));
        }
        return Result.Ok(result);
    }

    public async Task<Result> UpsertSecurityAsync(SecurityEngine security, CancellationToken cancellationToken)
    {
        try
        {
            var builder = Builders<SecurityEngine>.Filter;
            var filter = builder.Eq(o => o.Id, security.Id);

            var resultReplace = await _context.SecurityEngine.ReplaceOneAsync(filter,
                replacement: security,
                options: new ReplaceOptions { IsUpsert = true },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return Result.Fail(new Error(ex.Message));
        }
        return Result.Ok();
    }

    public async Task<Result> RemoveSecurityAsync(SecurityEngine security, CancellationToken cancellationToken)
    {
        try
        {
            var builder = Builders<SecurityEngine>.Filter;
            var filter = builder.Eq(o => o.Id, security.Id);

            var resultReplace = await _context.SecurityEngine.DeleteOneAsync(filter,
                //options: new DeleteOptions {    IsUpsert = true },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return Result.Fail(new Error(ex.Message));
        }
        return Result.Ok();
    }


}