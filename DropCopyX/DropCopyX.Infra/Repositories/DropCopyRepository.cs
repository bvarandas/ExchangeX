﻿using DropCopyX.Core.Repositories;
using DropCopyX.Infra.Data;
using FluentResults;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SharedX.Core.Matching.DropCopy;

namespace DropCopyX.Infra.Repositories;
public class DropCopyRepository : IDropCopyRepository
{
    private readonly IDropCopyContext _context;
    private readonly ILogger<DropCopyRepository> _logger;

    public DropCopyRepository(IDropCopyContext context, ILogger<DropCopyRepository> logger)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<Result> AddExecutionReports(IList<ExecutionReport> executions, CancellationToken cancellation)
    {
        Result result = null;
        bool resultInsert = false;
        try
        {
            var inserts = new List<WriteModel<ExecutionReport>>();

            foreach (var execution in executions)
                inserts.Add(new InsertOneModel<ExecutionReport>(execution));

            var insertResult = await _context.ExecutionReport.BulkWriteAsync(inserts, null, cancellation);
            resultInsert = insertResult.IsAcknowledged && insertResult.ModifiedCount > 0;
            result = Result.Ok();
        }
        catch (Exception ex)
        {
            //result.Errors.Add(new Error(ex.Message));
            _logger.LogError(ex.Message, ex);
            result = Result.Fail(new Error(ex.Message));
        }
        return result;
    }
}