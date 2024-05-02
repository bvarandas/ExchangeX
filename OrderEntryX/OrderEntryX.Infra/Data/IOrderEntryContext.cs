using MongoDB.Driver;
using OrderEntryX.Core.Entities;
namespace OrderEntryX.Infra.Data;
public interface IOrderEntryContext
{
    IMongoCollection<OrderEntry> OrderEntry { get; }
    IMongoCollection<Login> Login { get; }
}