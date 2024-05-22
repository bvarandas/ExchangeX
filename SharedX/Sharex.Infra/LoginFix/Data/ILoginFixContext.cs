using MongoDB.Driver;
using SharedX.Core.Entities;
namespace Sharedx.Infra.LoginFix.Data;
public interface ILoginFixContext
{
    IMongoCollection<Login> Login { get; }
}
