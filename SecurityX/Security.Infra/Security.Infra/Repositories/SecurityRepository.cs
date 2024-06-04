using FluentResults;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Security.Infra.Data;
using SecurityX.Core.Interfaces;
using SharedX.Core.Entities;
namespace Security.Infra.Repositories;

public class SecurityRepository : ISecurityRepository
{
    private readonly ISecurityContext _context;
    private readonly ILogger<SecurityRepository> _logger;

    public SecurityRepository(ISecurityContext context, ILogger<SecurityRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Dictionary<string, SecurityEngine>> GetAllSecurityiesAsync(CancellationToken cancellation)
    {
        Dictionary<string, SecurityEngine> result = null!;
        try
        {
            var builder = Builders<SecurityEngine>.Filter;
            var filter = builder.Empty;
            var securities = _context.SecurityEngine.Find(filter).ToEnumerable();
            foreach (var security in securities)
                result.Add(security.SecurityID, security);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }
        return result;
    }

    public async Task<Result<bool>> UpsertSecurityAsync(SecurityEngine security, CancellationToken cancellationToken)
    {
        bool result = false;
        try
        {
            var builder = Builders<SecurityEngine>.Filter;
            var filter = builder.Eq(o => o.SecurityID, security.SecurityID);

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
        return Result.Ok(result);
    }
}