using DropCopyX.Core.Entities;
using MongoDB.Driver;
using SharedX.Core.Matching.DropCopy;

namespace DropCopyX.Infra.Data;
public interface IDropCopyContext
{
    IMongoCollection<ExecutionReport> ExecutionReport { get; }

    IMongoCollection<Login> Login { get; }
}