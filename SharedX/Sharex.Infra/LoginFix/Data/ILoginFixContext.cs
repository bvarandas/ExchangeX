using MongoDB.Driver;
using SharedX.Core.Entities;
namespace Sharex.Infra.LoginFix.Data;
public interface ILoginFixContext
{
    IMongoCollection<Login> Login { get; }
}
