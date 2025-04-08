using FluentResults;
using MatchingX.Core.Entities;
using MatchingX.Core.Interfaces;
using MatchingX.Infra.Data;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
namespace MatchingX.Infra.Repositories;
public class MatchingRepository : IMatchingRepository
{
    private readonly IMatchingContext _context;
    private readonly ILogger<MatchingRepository> _logger;
    public MatchingRepository(IMatchingContext context, ILogger<MatchingRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> UpsertOrderMatchingAsync(MatchOrder order, CancellationToken cancellation)
    {
        bool result = false;
        var clientSessionOptions = new ClientSessionOptions();
        using (var session = await _context.MongoClient.StartSessionAsync(clientSessionOptions, cancellation))
        {
            session.StartTransaction();
            try
            {
                var builder = Builders<MatchOrder>.Filter;
                var filter = builder.Eq(o => o.OrderID, order.OrderID);

                var resultReplace = await _context.Matching.ReplaceOneAsync(session,
                filter,
                replacement: order,
                options: new ReplaceOptions { IsUpsert = true },
                cancellation);

                result = resultReplace.IsAcknowledged && resultReplace.ModifiedCount > 0;

                await session.CommitTransactionAsync();
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex.Message, ex);
                await session.AbortTransactionAsync();
                return Result.Fail(new Error(ex.Message));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                await session.AbortTransactionAsync();
                return Result.Fail(new Error(ex.Message));
            }
        }
        return Result.Ok();
    }

    public async Task<Result> RemoveOrdersMatchingAsync(List<long> IdOrders, CancellationToken cancellation)
    {
        bool result = false;
        var clientSessionOptions = new ClientSessionOptions();
        using (var session = await _context.MongoClient.StartSessionAsync(clientSessionOptions, cancellation))
        {
            session.StartTransaction();
            try
            {
                var filterDelete = Builders<MatchOrder>.Filter
                            .In(o => o.OrderID, IdOrders);

                var resultDelete = await _context.Matching.DeleteManyAsync(session, filterDelete);
                result = resultDelete.IsAcknowledged && resultDelete.DeletedCount > 0;

                await session.CommitTransactionAsync();
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex.Message, ex);
                await session.AbortTransactionAsync();
                return Result.Fail(new Error(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                await session.AbortTransactionAsync();
                return Result.Fail(new Error(ex.Message));
            }
        }
        return Result.Ok();
    }





}
